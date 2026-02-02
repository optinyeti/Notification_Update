using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User,Admin,SuperAdmin")]
public class FormController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public FormController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    // GET: Form - List all forms
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var forms = await _context.WebsiteForms
            .Where(f => f.TenantId == user.TenantId)
            .Include(f => f.CreatedBy)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        // Get stats
        ViewBag.TotalForms = forms.Count;
        ViewBag.PublishedForms = forms.Count(f => f.Status == FormStatus.Published);
        ViewBag.TotalSubmissions = forms.Sum(f => f.Submissions);
        ViewBag.TotalViews = forms.Sum(f => f.Views);

        return View(forms);
    }

    // GET: Form/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get pipelines for lead assignment
        var pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .Include(p => p.Stages)
            .ToListAsync();

        ViewBag.Pipelines = pipelines;

        return View(new WebsiteForm());
    }

    // POST: Form/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WebsiteForm form)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        form.TenantId = user.TenantId;
        form.CreatedById = user.Id;
        form.EmbedCode = Guid.NewGuid().ToString("N");
        form.CreatedAt = DateTime.UtcNow;
        form.UpdatedAt = DateTime.UtcNow;

        _context.WebsiteForms.Add(form);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Builder), new { id = form.Id });
    }

    // GET: Form/Builder/5 - Form Builder
    public async Task<IActionResult> Builder(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .Include(f => f.Fields.OrderBy(field => field.Order))
            .Include(f => f.Pipeline)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        // Get pipelines
        var pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .Include(p => p.Stages)
            .ToListAsync();

        ViewBag.Pipelines = pipelines;

        return View(form);
    }

    // POST: Form/SaveFields - Save form fields via AJAX
    [HttpPost("Form/SaveFields/{formId}")]
    public async Task<IActionResult> SaveFields(int formId, [FromBody] JsonElement fieldsJson)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var form = await _context.WebsiteForms
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == formId && f.TenantId == user.TenantId);

            if (form == null) return NotFound();

            // Remove existing fields
            _context.FormFields.RemoveRange(form.Fields);

            // Parse fields from JSON
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            foreach (var fieldJson in fieldsJson.EnumerateArray())
            {
                var field = new FormField
                {
                    FormId = formId,
                    FieldName = fieldJson.TryGetProperty("fieldName", out var fn) ? fn.GetString() ?? "" : "",
                    Label = fieldJson.TryGetProperty("label", out var lbl) ? lbl.GetString() ?? "" : "",
                    Placeholder = fieldJson.TryGetProperty("placeholder", out var ph) ? ph.GetString() : null,
                    HelpText = fieldJson.TryGetProperty("helpText", out var ht) ? ht.GetString() : null,
                    IsRequired = fieldJson.TryGetProperty("isRequired", out var req) && req.GetBoolean(),
                    Options = fieldJson.TryGetProperty("options", out var opt) ? opt.GetString() : null,
                    MinLength = fieldJson.TryGetProperty("minLength", out var min) && min.ValueKind == JsonValueKind.Number ? min.GetInt32() : null,
                    MaxLength = fieldJson.TryGetProperty("maxLength", out var max) && max.ValueKind == JsonValueKind.Number ? max.GetInt32() : null,
                    ValidationPattern = fieldJson.TryGetProperty("validationPattern", out var vp) ? vp.GetString() : null,
                    ValidationMessage = fieldJson.TryGetProperty("validationMessage", out var vm) ? vm.GetString() : null,
                    LeadFieldMapping = fieldJson.TryGetProperty("leadFieldMapping", out var lfm) ? lfm.GetString() : null,
                    Width = fieldJson.TryGetProperty("width", out var w) && w.ValueKind == JsonValueKind.Number ? w.GetInt32() : 100,
                    Order = fieldJson.TryGetProperty("order", out var ord) && ord.ValueKind == JsonValueKind.Number ? ord.GetInt32() : 0,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Parse field type (handle both string and number)
                if (fieldJson.TryGetProperty("fieldType", out var ft))
                {
                    if (ft.ValueKind == JsonValueKind.String)
                    {
                        var typeStr = ft.GetString() ?? "Text";
                        // Convert first letter to uppercase for enum parsing
                        typeStr = char.ToUpper(typeStr[0]) + typeStr.Substring(1).ToLower();
                        if (Enum.TryParse<FormFieldType>(typeStr, true, out var fieldType))
                            field.FieldType = fieldType;
                    }
                    else if (ft.ValueKind == JsonValueKind.Number)
                    {
                        field.FieldType = (FormFieldType)ft.GetInt32();
                    }
                }

                _context.FormFields.Add(field);
            }

            form.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // POST: Form/SaveSettings - Save form settings via AJAX
    [HttpPost("Form/SaveSettings/{formId}")]
    public async Task<IActionResult> SaveSettings(int formId, [FromBody] WebsiteForm settings)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .FirstOrDefaultAsync(f => f.Id == formId && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        // Update settings
        form.Name = settings.Name;
        form.Description = settings.Description;
        form.FormType = settings.FormType;
        form.Style = settings.Style;
        form.Theme = settings.Theme;
        form.BackgroundColor = settings.BackgroundColor;
        form.TextColor = settings.TextColor;
        form.ButtonColor = settings.ButtonColor;
        form.ButtonTextColor = settings.ButtonTextColor;
        form.BorderColor = settings.BorderColor;
        form.BorderRadius = settings.BorderRadius;
        form.FontFamily = settings.FontFamily;
        form.FontSize = settings.FontSize;
        form.CustomCss = settings.CustomCss;
        form.HeaderText = settings.HeaderText;
        form.SubheaderText = settings.SubheaderText;
        form.SubmitButtonText = settings.SubmitButtonText;
        form.FooterText = settings.FooterText;
        form.ShowPoweredBy = settings.ShowPoweredBy;
        form.SubmissionAction = settings.SubmissionAction;
        form.SuccessMessage = settings.SuccessMessage;
        form.RedirectUrl = settings.RedirectUrl;
        form.SendNotificationEmail = settings.SendNotificationEmail;
        form.NotificationEmail = settings.NotificationEmail;
        form.CreateLead = settings.CreateLead;
        form.PipelineId = settings.PipelineId;
        form.DefaultStageId = settings.DefaultStageId;
        form.LeadSource = settings.LeadSource;
        form.EnableHoneypot = settings.EnableHoneypot;
        form.EnableRecaptcha = settings.EnableRecaptcha;
        form.RequireDoubleOptIn = settings.RequireDoubleOptIn;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // POST: Form/Publish/5
    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        form.Status = FormStatus.Published;
        form.PublishedAt = DateTime.UtcNow;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // POST: Form/Unpublish/5
    [HttpPost]
    public async Task<IActionResult> Unpublish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        form.Status = FormStatus.Draft;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // GET: Form/Submissions/5
    public async Task<IActionResult> Submissions(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        var submissions = await _context.FormSubmissions
            .Where(s => s.FormId == id)
            .Include(s => s.Lead)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        ViewBag.Form = form;
        ViewBag.Submissions = submissions;

        return View(form);
    }

    // GET: Form/SubmissionDetail/5
    public async Task<IActionResult> SubmissionDetail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var submission = await _context.FormSubmissions
            .Include(s => s.Form)
                .ThenInclude(f => f!.Fields)
            .Include(s => s.Lead)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == user.TenantId);

        if (submission == null) return NotFound();

        // Mark as read
        if (submission.Status == SubmissionStatus.New)
        {
            submission.Status = SubmissionStatus.Read;
            await _context.SaveChangesAsync();
        }

        return View(submission);
    }

    // GET: Form/Analytics/5
    public async Task<IActionResult> Analytics(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var analytics = await _context.FormAnalytics
            .Where(a => a.FormId == id && a.Date >= startDate && a.Date <= endDate)
            .OrderBy(a => a.Date)
            .ToListAsync();

        // Get submission breakdown by day
        var submissionsByDay = await _context.FormSubmissions
            .Where(s => s.FormId == id && s.SubmittedAt >= startDate)
            .GroupBy(s => s.SubmittedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Get field completion rates
        var submissions = await _context.FormSubmissions
            .Where(s => s.FormId == id)
            .Select(s => s.Data)
            .ToListAsync();

        var fields = await _context.FormFields
            .Where(f => f.FormId == id)
            .OrderBy(f => f.Order)
            .ToListAsync();

        ViewBag.Form = form;
        ViewBag.Analytics = analytics;
        ViewBag.SubmissionsByDay = JsonSerializer.Serialize(submissionsByDay);
        ViewBag.Fields = fields;

        return View(form);
    }

    // GET: Form/Embed/5
    public async Task<IActionResult> Embed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        var tenant = await _context.Tenants.FindAsync(user.TenantId);
        var baseUrl = tenant?.PublicHostUrl ?? $"{Request.Scheme}://{Request.Host}";

        ViewBag.BaseUrl = baseUrl;
        ViewBag.Form = form;

        return View(form);
    }

    // GET: Form/Preview/5
    public async Task<IActionResult> Preview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .Include(f => f.Fields.OrderBy(field => field.Order))
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        return View(form);
    }

    // POST: Form/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var form = await _context.WebsiteForms
            .Include(f => f.Fields)
            .Include(f => f.FormSubmissions)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (form == null) return NotFound();

        _context.FormFields.RemoveRange(form.Fields);
        _context.FormSubmissions.RemoveRange(form.FormSubmissions);
        _context.WebsiteForms.Remove(form);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Form/Duplicate/5
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var original = await _context.WebsiteForms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == user.TenantId);

        if (original == null) return NotFound();

        var copy = new WebsiteForm
        {
            TenantId = user.TenantId,
            Name = $"{original.Name} (Copy)",
            Description = original.Description,
            FormType = original.FormType,
            Style = original.Style,
            Theme = original.Theme,
            BackgroundColor = original.BackgroundColor,
            TextColor = original.TextColor,
            ButtonColor = original.ButtonColor,
            ButtonTextColor = original.ButtonTextColor,
            BorderColor = original.BorderColor,
            BorderRadius = original.BorderRadius,
            FontFamily = original.FontFamily,
            FontSize = original.FontSize,
            CustomCss = original.CustomCss,
            HeaderText = original.HeaderText,
            SubheaderText = original.SubheaderText,
            SubmitButtonText = original.SubmitButtonText,
            FooterText = original.FooterText,
            ShowPoweredBy = original.ShowPoweredBy,
            SubmissionAction = original.SubmissionAction,
            SuccessMessage = original.SuccessMessage,
            RedirectUrl = original.RedirectUrl,
            SendNotificationEmail = original.SendNotificationEmail,
            NotificationEmail = original.NotificationEmail,
            CreateLead = original.CreateLead,
            PipelineId = original.PipelineId,
            DefaultStageId = original.DefaultStageId,
            LeadSource = original.LeadSource,
            EnableHoneypot = original.EnableHoneypot,
            EnableRecaptcha = original.EnableRecaptcha,
            RequireDoubleOptIn = original.RequireDoubleOptIn,
            Status = FormStatus.Draft,
            CreatedById = user.Id,
            EmbedCode = Guid.NewGuid().ToString("N")
        };

        _context.WebsiteForms.Add(copy);
        await _context.SaveChangesAsync();

        // Copy fields
        foreach (var field in original.Fields)
        {
            var fieldCopy = new FormField
            {
                FormId = copy.Id,
                FieldName = field.FieldName,
                Label = field.Label,
                Placeholder = field.Placeholder,
                HelpText = field.HelpText,
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                MinLength = field.MinLength,
                MaxLength = field.MaxLength,
                ValidationPattern = field.ValidationPattern,
                ValidationMessage = field.ValidationMessage,
                Options = field.Options,
                AllowOther = field.AllowOther,
                HasConditionalLogic = field.HasConditionalLogic,
                ConditionalLogic = field.ConditionalLogic,
                Order = field.Order,
                Width = field.Width,
                IsHidden = field.IsHidden,
                DefaultValue = field.DefaultValue,
                PreFillFromUrl = field.PreFillFromUrl,
                LeadFieldMapping = field.LeadFieldMapping
            };
            _context.FormFields.Add(fieldCopy);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Builder), new { id = copy.Id });
    }
}
