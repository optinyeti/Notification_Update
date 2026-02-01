using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;

namespace Notification_Application.Controllers;

/// <summary>
/// UserDashboardController handles user-specific operations like viewing popups,
/// subscription status, usage statistics, and support tickets.
/// This is separated from AdminController which handles tenant management and team management.
/// </summary>
[Authorize(Roles = "User,Admin,SuperAdmin")]
public class UserDashboardController : Controller
{
    private readonly IBillingService _billingService;
    private readonly ITenantService _tenantService;
    private readonly IPopupService _popupService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ISupportService _supportService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public UserDashboardController(
        IBillingService billingService,
        ITenantService tenantService,
        IPopupService popupService,
        IAnalyticsService analyticsService,
        ISupportService supportService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _billingService = billingService;
        _tenantService = tenantService;
        _popupService = popupService;
        _analyticsService = analyticsService;
        _supportService = supportService;
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// User dashboard - Shows usage stats, active popups, and quick actions
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var usage = await _billingService.GetUsageStatsAsync(user.TenantId);
        var popups = await _popupService.GetPopupsAsync(user.TenantId);
        var analytics = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);

        var model = new UserDashboardViewModel
        {
            UsageStats = usage,
            AnalyticsSummary = analytics,
            RecentPopups = popups.Take(5).ToList(),
            TotalPopups = popups.Count()
        };

        return View(model);
    }

    /// <summary>
    /// View current subscription plan and usage limits
    /// </summary>
    public async Task<IActionResult> Subscription()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var billing = await _billingService.GetBillingInfoAsync(user.TenantId);
        var billingHistory = await _billingService.GetBillingHistoryAsync(user.TenantId);

        ViewBag.BillingHistory = billingHistory;
        return View(billing);
    }

    /// <summary>
    /// View usage statistics with charts and breakdowns
    /// </summary>
    public async Task<IActionResult> Usage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var usage = await _billingService.GetUsageStatsAsync(user.TenantId);
        var featureUsage = await _billingService.GetFeatureUsageAsync(user.TenantId);
        
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var analytics = await _analyticsService.GetTenantAnalyticsAsync(user.TenantId, startDate, endDate);

        var model = new UsageDashboardViewModel
        {
            UsageStats = usage,
            FeatureUsage = featureUsage,
            Analytics = analytics.ToList(),
            DateRange = $"{startDate:MMM d} - {endDate:MMM d, yyyy}"
        };

        return View(model);
    }

    /// <summary>
    /// View popups created by the user's team
    /// </summary>
    public async Task<IActionResult> Popups()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popups = await _popupService.GetPopupsAsync(user.TenantId);
        
        return View(popups);
    }

    /// <summary>
    /// View analytics for all popups
    /// </summary>
    public async Task<IActionResult> Analytics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Check if user's plan includes analytics
        var canUseAnalytics = await _billingService.CanUseAnalyticsAsync(user.TenantId);
        if (!canUseAnalytics)
        {
            TempData["Warning"] = "Analytics feature is not available on your current plan.";
            return RedirectToAction("Subscription");
        }

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var analytics = await _analyticsService.GetTenantAnalyticsAsync(user.TenantId, startDate, endDate);
        var summary = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);

        var model = new AnalyticsViewModel
        {
            Summary = summary,
            PopupAnalytics = analytics.ToList(),
            StartDate = startDate,
            EndDate = endDate
        };

        return View(model);
    }

    /// <summary>
    /// View support tickets created by user
    /// </summary>
    public async Task<IActionResult> Support()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tickets = await _supportService.GetTicketsAsync(user.TenantId);
        return View(tickets);
    }

    /// <summary>
    /// Create a new support ticket
    /// </summary>
    [HttpGet]
    public IActionResult CreateSupportTicket()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupportTicket(CreateSupportTicketViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var ticket = new SupportTicket
        {
            Subject = model.Subject,
            Description = model.Description,
            Category = model.Category,
            Priority = model.Priority,
            TenantId = user.TenantId,
            CreatedById = user.Id
        };

        await _supportService.CreateTicketAsync(ticket);
        TempData["Success"] = "Support ticket created successfully!";

        return RedirectToAction("Support");
    }

    /// <summary>
    /// View single support ticket and add messages
    /// </summary>
    public async Task<IActionResult> SupportTicket(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var ticket = await _supportService.GetTicketAsync(id, user.TenantId);
        if (ticket == null) return NotFound();

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketMessage(int ticketId, string message)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _supportService.AddMessageAsync(ticketId, message, user.Id, false);
        TempData["Success"] = "Message added successfully!";

        return RedirectToAction("SupportTicket", new { id = ticketId });
    }

    /// <summary>
    /// Check if user can create more popups based on plan limits
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CanCreatePopup(int tenantId)
    {
        var canCreate = await _billingService.CanCreatePopupAsync(tenantId);
        var remaining = await _billingService.GetRemainingPopupsAsync(tenantId);

        return Json(new
        {
            success = canCreate,
            message = canCreate 
                ? "You can create a new popup" 
                : "You have reached your popup limit. Please upgrade your plan.",
            remaining = remaining
        });
    }

    /// <summary>
    /// Check feature access for current plan
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckFeatureAccess(int tenantId, string feature)
    {
        var hasAccess = feature.ToLower() switch
        {
            "advanced_targeting" => await _billingService.CanUseAdvancedTargetingAsync(tenantId),
            "analytics" => await _billingService.CanUseAnalyticsAsync(tenantId),
            "api" => await _billingService.CanUseApiAsync(tenantId),
            _ => false
        };

        return Json(new { hasAccess });
    }

    /// <summary>
    /// Get current usage for API/programmatic access
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetUsageStats(int tenantId)
    {
        var stats = await _billingService.GetUsageStatsAsync(tenantId);
        return Json(stats);
    }

    /// <summary>
    /// Render popups as a count summary for dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PopupCountSummary()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popups = await _popupService.GetPopupsAsync(user.TenantId);
        var usage = await _billingService.GetUsageStatsAsync(user.TenantId);

        return Json(new
        {
            total = popups.Count(),
            active = popups.Count(p => p.Status == PopupStatus.Published),
            draft = popups.Count(p => p.Status == PopupStatus.Draft),
            remaining = usage.PopupsRemaining
        });
    }

    /// <summary>
    /// Get popup analytics summary for dashboard cards
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AnalyticsSummary()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var analytics = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);
        return Json(analytics);
    }

    /// <summary>
    /// View and manage captured leads from all popups
    /// </summary>
    public async Task<IActionResult> Leads(int? popupId, string? search, int page = 1, int pageSize = 50)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.EmailCaptures
            .Include(e => e.Popup)
            .Where(e => e.TenantId == user.TenantId)
            .OrderByDescending(e => e.CapturedAt);

        // Filter by popup if specified
        if (popupId.HasValue)
        {
            query = (IOrderedQueryable<EmailCapture>)query.Where(e => e.PopupId == popupId.Value);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = (IOrderedQueryable<EmailCapture>)query.Where(e => 
                e.Email.Contains(search) || 
                (e.FirstName != null && e.FirstName.Contains(search)) ||
                (e.LastName != null && e.LastName.Contains(search)) ||
                (e.Phone != null && e.Phone.Contains(search)));
        }

        var totalLeads = await query.CountAsync();
        var leads = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var popups = await _context.Popups
            .Where(p => p.TenantId == user.TenantId)
            .OrderBy(p => p.Name)
            .ToListAsync();

        var model = new LeadsViewModel
        {
            Leads = leads,
            TotalLeads = totalLeads,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalLeads / (double)pageSize),
            Popups = popups,
            SelectedPopupId = popupId,
            SearchQuery = search
        };

        return View(model);
    }

    /// <summary>
    /// Export leads to CSV
    /// </summary>
    public async Task<IActionResult> ExportLeads(int? popupId, string? search)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.EmailCaptures
            .Include(e => e.Popup)
            .Where(e => e.TenantId == user.TenantId)
            .OrderByDescending(e => e.CapturedAt);

        if (popupId.HasValue)
        {
            query = (IOrderedQueryable<EmailCapture>)query.Where(e => e.PopupId == popupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = (IOrderedQueryable<EmailCapture>)query.Where(e => 
                e.Email.Contains(search) || 
                (e.FirstName != null && e.FirstName.Contains(search)) ||
                (e.LastName != null && e.LastName.Contains(search)));
        }

        var leads = await query.ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Email,First Name,Last Name,Phone,Popup,Captured At,IP Address,Source,Consent Given");

        foreach (var lead in leads)
        {
            csv.AppendLine($"\"{lead.Email}\",\"{lead.FirstName}\",\"{lead.LastName}\",\"{lead.Phone}\",\"{lead.Popup?.Name}\",\"{lead.CapturedAt:yyyy-MM-dd HH:mm:ss}\",\"{lead.IpAddress}\",\"{lead.Source}\",\"{lead.ConsentGiven}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"leads_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Delete a lead
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteLead(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.EmailCaptures
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == user.TenantId);

        if (lead == null) return NotFound();

        _context.EmailCaptures.Remove(lead);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}

// ViewModels for User Dashboard
public class UserDashboardViewModel
{
    public UsageStats? UsageStats { get; set; }
    public object? AnalyticsSummary { get; set; }
    public List<Popup> RecentPopups { get; set; } = new();
    public int TotalPopups { get; set; }
}

public class UsageDashboardViewModel
{
    public UsageStats? UsageStats { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
    public List<PopupAnalytics> Analytics { get; set; } = new();
    public string DateRange { get; set; } = string.Empty;
}

public class LeadsViewModel
{
    public List<EmailCapture> Leads { get; set; } = new();
    public int TotalLeads { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<Popup> Popups { get; set; } = new();
    public int? SelectedPopupId { get; set; }
    public string? SearchQuery { get; set; }
}
