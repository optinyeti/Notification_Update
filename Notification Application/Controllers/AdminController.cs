using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;

namespace Notification_Application.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ISupportService _supportService;
    private readonly IPopupTemplateService _templateService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(
        ITenantService tenantService,
        IAnalyticsService analyticsService,
        ISupportService supportService,
        IPopupTemplateService templateService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _tenantService = tenantService;
        _analyticsService = analyticsService;
        _supportService = supportService;
        _templateService = templateService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var analytics = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);
        var tickets = await _supportService.GetTicketsAsync(user.TenantId);

        var model = new AdminDashboardViewModel
        {
            AnalyticsSummary = analytics,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            TotalTickets = tickets.Count()
        };

        return View(model);
    }

    public async Task<IActionResult> Users()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        var users = tenant.Users.ToList();
        return View(users);
    }

    public async Task<IActionResult> Analytics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

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

    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        var model = new TenantSettingsViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Domain = tenant.Domain,
            Description = tenant.Description,
            PublicHostUrl = tenant.PublicHostUrl,
            StripeSecretKey = tenant.StripeSecretKey,
            StripePublishableKey = tenant.StripePublishableKey,
            StripeWebhookSecret = tenant.StripeWebhookSecret
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(TenantSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        tenant.Name = model.Name;
        tenant.Description = model.Description;
        tenant.PublicHostUrl = model.PublicHostUrl;
        tenant.StripeSecretKey = model.StripeSecretKey;
        tenant.StripePublishableKey = model.StripePublishableKey;
        tenant.StripeWebhookSecret = model.StripeWebhookSecret;

        await _tenantService.UpdateTenantAsync(tenant);
        TempData["Success"] = "Settings updated successfully!";

        return View(model);
    }

    public async Task<IActionResult> Subscription()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    public async Task<IActionResult> Support()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tickets = await _supportService.GetTicketsAsync(user.TenantId);
        return View(tickets);
    }

    [HttpGet]
    public IActionResult CreateSupportTicket()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupportTicket(CreateSupportTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

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

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AllTenants()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        return View(tenants);
    }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> TenantDetails(int id)
    {
        var tenant = await _tenantService.GetTenantAsync(id);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    [HttpPost]
    public async Task<IActionResult> SyncPopupCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var success = await _tenantService.SyncPopupCountAsync(user.TenantId);
        if (success)
        {
            TempData["Success"] = "Popup count synchronized with database successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to synchronize popup count.";
        }

        return RedirectToAction("Subscription");
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ResetPopupCount(int tenantId)
    {
        var success = await _tenantService.ResetPopupCountAsync(tenantId);
        if (success)
        {
            TempData["Success"] = "Popup count reset successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to reset popup count.";
        }

        return RedirectToAction("TenantDetails", new { id = tenantId });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SyncTenantPopupCount(int tenantId)
    {
        var success = await _tenantService.SyncPopupCountAsync(tenantId);
        if (success)
        {
            TempData["Success"] = "Popup count synchronized successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to synchronize popup count.";
        }

        return RedirectToAction("TenantDetails", new { id = tenantId });
    }
    
    [HttpGet]
    public async Task<IActionResult> Templates()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        return View(templates);
    }
    
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ImportDefaultTemplates()
    {
        try
        {
            // Clear existing templates (optional - comment out if you want to keep existing ones)
            // await _context.Database.ExecuteSqlRawAsync("DELETE FROM PopupTemplates");
            
            // Seed the templates
            await DatabaseSeeder.SeedPopupTemplatesAsync(_context);
            
            TempData["Success"] = "Default templates imported successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to import templates: {ex.Message}";
        }
        
        return RedirectToAction("Templates");
    }
    
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        try
        {
            var template = await _context.PopupTemplates.FindAsync(id);
            if (template != null)
            {
                _context.PopupTemplates.Remove(template);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Template deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Template not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete template: {ex.Message}";
        }
        
        return RedirectToAction("Templates");
    }
}