using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notification_Application.Controllers;

[Authorize]
public class PopupController : Controller
{
    private readonly IPopupService _popupService;
    private readonly IPopupTemplateService _templateService;
    private readonly IAnalyticsService _analyticsService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public PopupController(
        IPopupService popupService,
        IPopupTemplateService templateService,
        IAnalyticsService analyticsService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _popupService = popupService;
        _templateService = templateService;
        _analyticsService = analyticsService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popups = await _popupService.GetPopupsAsync(user.TenantId);
        // Compute public host for embed snippets
        var tenant = await _context.Tenants.FindAsync(user.TenantId);
        var publicHost = tenant?.PublicHostUrl
            ?? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        ViewBag.PublicHost = publicHost;

        return View(popups);
    }

    public IActionResult CreateCampaign()
    {
        return View();
    }

    public async Task<IActionResult> Playbooks()
    {
        var playbooks = await _context.Playbooks
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();
        return View(playbooks);
    }
    
    public async Task<IActionResult> PlaybookDetail(int id)
    {
        var playbook = await _context.Playbooks.FindAsync(id);
        if (playbook == null) return NotFound();
        
        var templateIds = playbook.GetTemplateIdList();
        var templates = await _context.PopupTemplates
            .Where(t => templateIds.Contains(t.Id))
            .ToListAsync();
            
        ViewBag.Playbook = playbook;
        return View(templates);
    }

    public async Task<IActionResult> Designer(int? id, string? name, PopupType? type, string? templateFile, string? category)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        Popup popup;

        if (id.HasValue)
        {
            // Editing existing popup
            popup = await _popupService.GetPopupAsync(id.Value, user.TenantId);
            if (popup == null) return NotFound();
        }
        else
        {
            // Create a new popup with the provided name and type
            popup = new Popup
            {
                Name = name ?? "New Campaign",
                Type = type ?? PopupType.EmailCollector,
                TenantId = user.TenantId,
                CreatedById = user.Id,
                Status = PopupStatus.Draft,
                Content = "{}",
                TargetingRules = "{}",
                Title = "Your Campaign Title",
                Subtitle = "Add your subtitle here",
                CallToAction = "Get Started",
                ShowOnMobile = true,
                ShowOnDesktop = true
            };

            // If templateFile is provided, load the HTML template from file system
            if (!string.IsNullOrEmpty(templateFile) && !string.IsNullOrEmpty(category))
            {
                // Capitalize first letter of category to match folder names (Popup, FloatingBar, etc.)
                var categoryFormatted = char.ToUpper(category[0]) + category.Substring(1).ToLower();
                
                // Handle special cases
                if (categoryFormatted == "Popup") categoryFormatted = "Popup";
                else if (categoryFormatted == "Floatingbar") categoryFormatted = "FloatingBar";
                else if (categoryFormatted == "Slidein") categoryFormatted = "SlideIn";
                
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", categoryFormatted, templateFile);
                
                if (System.IO.File.Exists(templatePath))
                {
                    var content = await System.IO.File.ReadAllTextAsync(templatePath);
                    // Strip Razor directives from .cshtml files
                    if (templateFile.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        content = StripRazorDirectives(content);
                    }
                    popup.Content = content;
                    popup.Name = Path.GetFileNameWithoutExtension(templateFile).Replace("-", " ")
                        .Replace("_", " ");
                    // Convert to title case
                    popup.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(popup.Name.ToLower());
                }
            }
            // If type is provided and name is not "Blank", try to load a template for that type
            else if (type.HasValue && !string.Equals(name, "Blank", StringComparison.OrdinalIgnoreCase))
            {
                var templates = await _templateService.GetTemplatesByTypeAsync(type.Value);
                var template = templates?.FirstOrDefault();
                
                if (template != null)
                {
                    // Load template data into the popup
                    popup.Title = template.Title ?? popup.Title;
                    popup.Subtitle = template.Subtitle ?? popup.Subtitle;
                    popup.CallToAction = template.CallToAction ?? popup.CallToAction;
                    popup.ImageUrl = template.ImageUrl;
                    popup.TargetingRules = template.DefaultTargetingRules ?? popup.TargetingRules;
                    popup.Trigger = template.DefaultTrigger;
                    popup.DelayMs = template.DefaultDelayMs;
                    popup.Frequency = template.DefaultFrequency;
                    
                    // Generate HTML content from template based on type
                    popup.Content = GenerateContentFromTemplate(template);
                }
            }
            
            // For "Blank" template, ensure truly empty content
            if (string.Equals(name, "Blank", StringComparison.OrdinalIgnoreCase))
            {
                popup.Content = "";
                popup.Title = "";
                popup.Subtitle = "";
            }
            
            // Save the new popup first
            try
            {
                await _popupService.CreatePopupAsync(popup);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Popup limit exceeded"))
            {
                TempData["Error"] = "You've reached your popup limit for your current subscription plan. Please upgrade your plan or delete some existing popups.";
                return RedirectToAction("Index");
            }
        }

        return View(popup);
    }

    public IActionResult SelectType()
    {
        return View();
    }

    public async Task<IActionResult> SelectTemplate(PopupType? type = null)
    {
        var templates = type.HasValue 
            ? await _templateService.GetTemplatesByTypeAsync(type.Value)
            : await _templateService.GetAllTemplatesAsync();
        ViewBag.PopupType = type;
        return View(templates);
    }

    public IActionResult Create(PopupType? type, int? templateId)
    {
        // Redirect to CreateCampaign instead
        return RedirectToAction("CreateCampaign");
    }

    [HttpGet]
    public async Task<IActionResult> CreateFromTemplate(int templateId)
    {
        var template = await _templateService.GetTemplateAsync(templateId);
        if (template == null) return NotFound();

        var model = new PopupCreateViewModel
        {
            Name = template.Name + " - Copy",
            Type = template.Type,
            Title = template.Title,
            Subtitle = template.Subtitle,
            CallToAction = template.CallToAction,
            Content = template.Content,
            TargetingRules = template.DefaultTargetingRules,
            Trigger = template.DefaultTrigger,
            DelayMs = template.DefaultDelayMs,
            Frequency = template.DefaultFrequency,
            TypeSpecificOptions = template.TypeSpecificOptions
        };

        return View("Create", model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PopupCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = new Popup
        {
            Name = model.Name,
            Type = model.Type,
            Title = model.Title,
            Subtitle = model.Subtitle,
            CallToAction = model.CallToAction,
            Content = model.Content,
            TargetingRules = model.TargetingRules,
            Trigger = model.Trigger,
            DelayMs = model.DelayMs,
            Frequency = model.Frequency,
            ShowOnMobile = model.ShowOnMobile,
            ShowOnDesktop = model.ShowOnDesktop,
            TenantId = user.TenantId,
            CreatedById = user.Id
        };

        try
        {
            await _popupService.CreatePopupAsync(popup);
            TempData["Success"] = "Popup created successfully!";
            return RedirectToAction("Edit", new { id = popup.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        var model = new PopupEditViewModel
        {
            Id = popup.Id,
            TenantId = popup.TenantId,
            Name = popup.Name,
            Type = popup.Type,
            Title = popup.Title,
            Subtitle = popup.Subtitle,
            CallToAction = popup.CallToAction,
            Content = popup.Content,
            TargetingRules = popup.TargetingRules,
            Trigger = popup.Trigger,
            DelayMs = popup.DelayMs,
            Frequency = popup.Frequency,
            ShowOnMobile = popup.ShowOnMobile,
            ShowOnDesktop = popup.ShowOnDesktop,
            Status = popup.Status
        };

        // Compute public host for embed snippets
        var tenant = await _context.Tenants.FindAsync(user.TenantId);
        var publicHost = tenant?.PublicHostUrl
            ?? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        ViewBag.PublicHost = publicHost;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PopupEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(model.Id, user.TenantId);
        if (popup == null) return NotFound();

        popup.Name = model.Name;
        popup.Title = model.Title;
        popup.Subtitle = model.Subtitle;
        popup.CallToAction = model.CallToAction;
        popup.Content = model.Content;
        popup.TargetingRules = model.TargetingRules;
        popup.Trigger = model.Trigger;
        popup.DelayMs = model.DelayMs;
        popup.Frequency = model.Frequency;
        popup.ShowOnMobile = model.ShowOnMobile;
        popup.ShowOnDesktop = model.ShowOnDesktop;

        await _popupService.UpdatePopupAsync(popup);
        TempData["Success"] = "Popup updated successfully!";
        
        return RedirectToAction("Edit", new { id = popup.Id });
    }

    [HttpPost]
    public async Task<IActionResult> SaveDesign(int id, [FromBody] PopupDesignModel design)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        // Update design content
        popup.Content = design.Content ?? "{}";
        popup.TargetingRules = design.TargetingRules ?? "{}";
        
        // Update display rules if provided
        if (!string.IsNullOrEmpty(design.Trigger))
        {
            if (Enum.TryParse<PopupTrigger>(design.Trigger, out var trigger))
                popup.Trigger = trigger;
        }
        
        if (!string.IsNullOrEmpty(design.Frequency))
        {
            if (Enum.TryParse<PopupFrequency>(design.Frequency, out var frequency))
                popup.Frequency = frequency;
        }
        
        if (design.DelayMs.HasValue)
            popup.DelayMs = design.DelayMs.Value;
        
        if (design.ShowOnMobile.HasValue)
            popup.ShowOnMobile = design.ShowOnMobile.Value;
        
        if (design.ShowOnDesktop.HasValue)
            popup.ShowOnDesktop = design.ShowOnDesktop.Value;

        await _popupService.UpdatePopupAsync(popup);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.PublishPopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup published successfully!";

        return RedirectToAction("Edit", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Unpublish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.UnpublishPopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup unpublished successfully!";

        return RedirectToAction("Edit", new { id });
    }

    public async Task<IActionResult> LivePreview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        return View(popup);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.DeletePopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup deleted successfully!";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Analytics(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        
        // Get daily analytics data
        var dailyAnalytics = await _analyticsService.GetDailyPopupAnalyticsAsync(id, startDate, endDate);

        var model = new PopupAnalyticsViewModel
        {
            Popup = popup,
            Analytics = dailyAnalytics,
            StartDate = startDate,
            EndDate = endDate
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetScript(int id)
    {
        var script = await _popupService.GeneratePopupScriptAsync(id);
        return Content(script, "application/javascript");
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetTenantScript/{tenantId}")]
    public async Task<IActionResult> GetTenantScript(int tenantId)
    {
        var script = await _popupService.GenerateTenantScriptAsync(tenantId);
        return Content(script, "application/javascript");
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetTenantPopups/{tenantId}")]
    public async Task<IActionResult> GetTenantPopups(int tenantId)
    {
        var popups = await _context.Popups
            .Where(p => p.TenantId == tenantId && p.Status == PopupStatus.Published)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                type = p.Type.ToString(),
                content = p.Content,
                trigger = p.Trigger.ToString(),
                delay = p.DelayMs,
                frequency = p.Frequency.ToString(),
                showOnMobile = p.ShowOnMobile,
                showOnDesktop = p.ShowOnDesktop,
                targetingRules = p.TargetingRules
            })
            .ToListAsync();

        return Json(popups);
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetPopupData/{id}")]
    public async Task<IActionResult> GetPopupData(int id)
    {
        // Get popup without tenant restriction for public embed
        var popup = await _context.Popups.FindAsync(id);
        if (popup == null || popup.Status != PopupStatus.Published)
            return NotFound();

        var data = new
        {
            id = popup.Id,
            name = popup.Name,
            type = popup.Type.ToString(),
            content = popup.Content,
            trigger = popup.Trigger.ToString(),
            delay = popup.DelayMs,
            frequency = popup.Frequency.ToString(),
            showOnMobile = popup.ShowOnMobile,
            showOnDesktop = popup.ShowOnDesktop
        };

        return Json(data);
    }

    public async Task<IActionResult> Preview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        return View(popup);
    }

    private string StripRazorDirectives(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        
        // Remove @{ Layout = null; } and similar Razor blocks at the start
        var patterns = new[]
        {
            @"@\{\s*Layout\s*=\s*null\s*;\s*\}\s*",  // @{ Layout = null; }
            @"@\{\s*\}\s*",  // Empty @{ }
            @"@model\s+[^\r\n]+\s*",  // @model directives
            @"@using\s+[^\r\n]+\s*",  // @using directives
            @"@inject\s+[^\r\n]+\s*"  // @inject directives
        };
        
        foreach (var pattern in patterns)
        {
            content = System.Text.RegularExpressions.Regex.Replace(content, pattern, "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return content.TrimStart();
    }

    private string GenerateContentFromTemplate(PopupTemplate template)
    {
        // Generate HTML content based on template type and options
        try
        {
            Console.WriteLine($"[GenerateContentFromTemplate] Template: {template.Name}");
            Console.WriteLine($"[GenerateContentFromTemplate] Content: {template.Content?.Substring(0, Math.Min(100, template.Content?.Length ?? 0))}...");
            Console.WriteLine($"[GenerateContentFromTemplate] TypeSpecificOptions: {template.TypeSpecificOptions?.Substring(0, Math.Min(100, template.TypeSpecificOptions?.Length ?? 0))}...");
            
            var html = "<div style=\"padding: 40px; text-align: center;\">";

            // Try to parse Content first
            Dictionary<string, JsonElement>? contentJson = null;
            if (!string.IsNullOrEmpty(template.Content) && template.Content != "{}")
            {
                try
                {
                    contentJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(template.Content);
                    Console.WriteLine($"[GenerateContentFromTemplate] Parsed Content JSON: {(contentJson != null ? string.Join(", ", contentJson.Keys) : "none")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GenerateContentFromTemplate] Failed to parse Content: {ex.Message}");
                }
            }

            // Try to parse TypeSpecificOptions
            Dictionary<string, JsonElement>? optionsJson = null;
            if (!string.IsNullOrEmpty(template.TypeSpecificOptions) && template.TypeSpecificOptions != "{}")
            {
                try
                {
                    optionsJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(template.TypeSpecificOptions);
                    Console.WriteLine($"[GenerateContentFromTemplate] Parsed Options JSON: {(optionsJson != null ? string.Join(", ", optionsJson.Keys) : "none")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GenerateContentFromTemplate] Failed to parse TypeSpecificOptions: {ex.Message}");
                }
            }

            // Helper to get string value from either source
            string GetValue(string key, string defaultValue = "")
            {
                if (contentJson != null && contentJson.ContainsKey(key))
                {
                    var value = contentJson[key].GetString() ?? defaultValue;
                    Console.WriteLine($"[GetValue] Found '{key}' in Content: {value}");
                    return value;
                }
                if (optionsJson != null && optionsJson.ContainsKey(key))
                {
                    var value = optionsJson[key].GetString() ?? defaultValue;
                    Console.WriteLine($"[GetValue] Found '{key}' in Options: {value}");
                    return value;
                }
                Console.WriteLine($"[GetValue] Key '{key}' not found, using default: {defaultValue}");
                return defaultValue;
            }

            // Get heading/title - check JSON first, then fallback to template properties
            var heading = GetValue("heading", 
                          GetValue("Heading", 
                          GetValue("title", "")));
            
            // Fallback to template-level Title or Name if no heading in JSON
            if (string.IsNullOrEmpty(heading))
            {
                heading = !string.IsNullOrEmpty(template.Title) ? template.Title : template.Name;
            }

            Console.WriteLine($"[GenerateContentFromTemplate] Final heading: {heading}");
            html += $"<h2 style=\"font-size: 28px; font-weight: 700; color: #1e293b; margin-bottom: 16px;\">{heading}</h2>";

            // Get description/subtitle - check JSON first, then fallback to template properties
            var description = GetValue("description", 
                              GetValue("Description", 
                              GetValue("subtitle", "")));
            
            // Fallback to template-level Subtitle or Description
            if (string.IsNullOrEmpty(description))
            {
                description = !string.IsNullOrEmpty(template.Subtitle) ? template.Subtitle : template.Description ?? "";
            }

            if (!string.IsNullOrEmpty(description))
            {
                html += $"<p style=\"font-size: 16px; color: #64748b; margin-bottom: 24px;\">{description}</p>";
            }

            // Add coupon code if present (for Coupon type)
            var couponCode = GetValue("couponCode", GetValue("CouponCode", ""));
            if (!string.IsNullOrEmpty(couponCode))
            {
                var discountValue = GetValue("discountValue", GetValue("DiscountValue", ""));
                if (!string.IsNullOrEmpty(discountValue))
                {
                    html += $"<div style=\"font-size: 18px; color: #10b981; font-weight: 600; margin: 16px 0;\">{discountValue}</div>";
                }
                html += $"<div style=\"background: #1e293b; color: white; padding: 16px 32px; border-radius: 8px; font-size: 24px; font-weight: 700; letter-spacing: 2px; margin: 24px auto; display: inline-block; max-width: 90%;\">{couponCode}</div>";
                
                var expiryNote = GetValue("expiryNote", "");
                if (!string.IsNullOrEmpty(expiryNote))
                {
                    html += $"<p style=\"font-size: 13px; color: #94a3b8; margin-top: 8px;\">{expiryNote}</p>";
                }
            }

            // Add email field for most templates
            if (template.Type == PopupType.EmailCollector || template.Type == PopupType.Coupon || 
                GetValue("requireEmail", GetValue("RequireEmail", "false")).ToLower() == "true")
            {
                var emailPlaceholder = GetValue("emailPlaceholder", GetValue("EmailPlaceholder", "Enter your email"));
                html += $"<div style=\"margin: 24px 0;\"><input type=\"email\" placeholder=\"{emailPlaceholder}\" style=\"width: 100%; max-width: 400px; padding: 12px 16px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 15px;\"></div>";
            }

            // Add button
            var buttonText = GetValue("buttonText", 
                             GetValue("ButtonText", 
                             GetValue("callToAction", "Get Started")));
            html += $"<div style=\"margin-top: 24px;\"><button style=\"background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); color: white; border: none; padding: 14px 32px; border-radius: 8px; font-size: 16px; font-weight: 600; cursor: pointer;\">{buttonText}</button></div>";

            // Add success message hint
            var successMessage = GetValue("successMessage", GetValue("SuccessMessage", ""));
            if (!string.IsNullOrEmpty(successMessage))
            {
                html += $"<p style=\"font-size: 13px; color: #94a3b8; margin-top: 16px; font-style: italic;\">On submit: \"{successMessage}\"</p>";
            }

            html += "</div>";

            return JsonSerializer.Serialize(new { html });
        }
        catch (Exception ex)
        {
            // Fallback to basic template with template name
            return JsonSerializer.Serialize(new { 
                html = $"<div style=\"padding: 40px; text-align: center;\"><h2 style=\"font-size: 28px; font-weight: 700; color: #1e293b; margin-bottom: 16px;\">{template.Name}</h2><p style=\"font-size: 16px; color: #64748b; margin-bottom: 24px;\">{template.Description}</p><div style=\"margin: 24px 0;\"><input type=\"email\" placeholder=\"Enter your email\" style=\"width: 100%; max-width: 400px; padding: 12px 16px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 15px;\"></div><button style=\"background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); color: white; border: none; padding: 14px 32px; border-radius: 8px; font-size: 16px; font-weight: 600; cursor: pointer;\">Get Started</button></div>" 
            });
        }
    }
}