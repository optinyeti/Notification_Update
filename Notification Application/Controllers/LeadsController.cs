using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User,Admin,SuperAdmin")]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public LeadsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Leads
    public async Task<IActionResult> Index(int? popupId, string? status, string? searchTerm, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .Include(l => l.Popup)
            .AsQueryable();

        // Filter by popup
        if (popupId.HasValue)
        {
            query = query.Where(l => l.PopupId == popupId.Value);
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, out var leadStatus))
        {
            query = query.Where(l => l.Status == leadStatus);
        }

        // Search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(l => 
                (l.Email != null && l.Email.Contains(searchTerm)) ||
                (l.FirstName != null && l.FirstName.Contains(searchTerm)) ||
                (l.LastName != null && l.LastName.Contains(searchTerm)) ||
                (l.Company != null && l.Company.Contains(searchTerm)) ||
                (l.Phone != null && l.Phone.Contains(searchTerm))
            );
        }

        // Get popups for filter dropdown
        var popups = await _context.Popups
            .Where(p => p.TenantId == user.TenantId)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        ViewBag.Popups = popups;
        ViewBag.CurrentPopupId = popupId;
        ViewBag.CurrentStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        // Pagination
        int pageSize = 50;
        var totalLeads = await query.CountAsync();
        var leads = await query
            .OrderByDescending(l => l.CapturedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalPages = (int)Math.Ceiling(totalLeads / (double)pageSize);
        ViewBag.TotalLeads = totalLeads;

        return View(leads);
    }

    // GET: Leads/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .Include(l => l.Popup)
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        // Parse custom fields
        try
        {
            var customFields = JsonSerializer.Deserialize<Dictionary<string, object>>(lead.CustomFields);
            ViewBag.CustomFields = customFields;
        }
        catch
        {
            ViewBag.CustomFields = new Dictionary<string, object>();
        }

        return View(lead);
    }

    // POST: Leads/UpdateStatus
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, LeadStatus status, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        lead.Status = status;
        if (!string.IsNullOrEmpty(notes))
        {
            lead.Notes = notes;
        }
        
        if (status == LeadStatus.Contacted)
        {
            lead.LastContactedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Leads/Export
    public async Task<IActionResult> Export(int? popupId, string? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .Include(l => l.Popup)
            .AsQueryable();

        if (popupId.HasValue)
        {
            query = query.Where(l => l.PopupId == popupId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, out var leadStatus))
        {
            query = query.Where(l => l.Status == leadStatus);
        }

        var leads = await query.OrderByDescending(l => l.CapturedAt).ToListAsync();

        // Generate CSV
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Date,Email,First Name,Last Name,Phone,Company,Source,Popup,Status,UTM Source,UTM Campaign");

        foreach (var lead in leads)
        {
            csv.AppendLine($"{lead.CapturedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"\"{lead.Email}\"," +
                          $"\"{lead.FirstName}\"," +
                          $"\"{lead.LastName}\"," +
                          $"\"{lead.Phone}\"," +
                          $"\"{lead.Company}\"," +
                          $"\"{lead.Source}\"," +
                          $"\"{lead.Popup?.Name}\"," +
                          $"{lead.Status}," +
                          $"\"{lead.UtmSource}\"," +
                          $"\"{lead.UtmCampaign}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"leads_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // API: Capture lead from popup
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Capture([FromBody] LeadCaptureRequest request)
    {
        if (request == null || request.PopupId <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        var popup = await _context.Popups
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(p => p.Id == request.PopupId);

        if (popup == null)
        {
            return NotFound(new { success = false, message = "Popup not found" });
        }

        // Create lead
        var lead = new Lead
        {
            PopupId = request.PopupId,
            TenantId = popup.TenantId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Company = request.Company,
            CustomFields = JsonSerializer.Serialize(request.CustomFields ?? new Dictionary<string, object>()),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            Source = request.Source,
            Referrer = Request.Headers["Referer"].ToString(),
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            UtmTerm = request.UtmTerm,
            UtmContent = request.UtmContent,
            ConsentGiven = request.ConsentGiven,
            ConsentDate = request.ConsentGiven ? DateTime.UtcNow : null,
            ConsentText = request.ConsentText,
            ViewType = request.ViewType,
            Status = LeadStatus.New
        };

        _context.Leads.Add(lead);
        
        // Update popup conversions
        popup.Conversions++;
        
        await _context.SaveChangesAsync();

        return Ok(new { success = true, leadId = lead.Id, message = "Lead captured successfully" });
    }
}

// Request model for lead capture
public class LeadCaptureRequest
{
    public int PopupId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public Dictionary<string, object>? CustomFields { get; set; }
    public string? Source { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    public bool ConsentGiven { get; set; }
    public string? ConsentText { get; set; }
    public string? ViewType { get; set; }
}
