using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;

namespace Notification_Application.Controllers;

/// <summary>
/// SuperAdminController handles platform-level administration tasks like managing
/// subscription plans, viewing all tenants, and platform-wide settings.
/// Only accessible to SuperAdmin users.
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SuperAdminController(
        ITenantService tenantService,
        UserManager<User> userManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _tenantService = tenantService;
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Platform admin dashboard - Overview of all tenants and system health
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalPopups = await _context.Popups.CountAsync();
        var activePlans = await _context.SubscriptionPlans.Where(p => p.IsActive).CountAsync();

        var model = new PlatformAdminDashboardViewModel
        {
            TotalTenants = tenants.Count(),
            ActiveTenants = tenants.Count(t => t.IsActive),
            TotalUsers = totalUsers,
            TotalPopups = totalPopups,
            ActiveSubscriptions = tenants.Count(t => t.SubscriptionStatus == "active"),
            AvailablePlans = activePlans
        };

        return View(model);
    }

    /// <summary>
    /// View all tenants in the system
    /// </summary>
    public async Task<IActionResult> AllTenants()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        return View(tenants);
    }

    /// <summary>
    /// View details and usage for a specific tenant
    /// </summary>
    public async Task<IActionResult> TenantDetails(int id)
    {
        var tenant = await _tenantService.GetTenantAsync(id);
        if (tenant == null) return NotFound();

        var users = await _context.Users.Where(u => u.TenantId == id).ToListAsync();
        var popups = await _context.Popups.Where(p => p.TenantId == id).ToListAsync();

        ViewBag.Users = users;
        ViewBag.PopupCount = popups.Count;
        ViewBag.ActivePopups = popups.Count(p => p.Status == PopupStatus.Published);

        return View(tenant);
    }

    /// <summary>
    /// Manage subscription plans available to tenants
    /// </summary>
    public async Task<IActionResult> Plans()
    {
        var plans = await _context.SubscriptionPlans.ToListAsync();
        plans = plans.OrderBy(p => p.MonthlyPrice).ToList();
        return View(plans);
    }

    /// <summary>
    /// Create new subscription plan
    /// </summary>
    [HttpGet]
    public IActionResult CreatePlan()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlan(SubscriptionPlan plan)
    {
        if (ModelState.IsValid)
        {
            plan.CreatedAt = DateTime.UtcNow;
            plan.IsActive = true;
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Plan created successfully!";
            return RedirectToAction("Plans");
        }
        return View(plan);
    }

    /// <summary>
    /// Edit existing subscription plan
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditPlan(int id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null)
        {
            TempData["Error"] = "Plan not found!";
            return RedirectToAction("Plans");
        }
        return View(plan);
    }

    [HttpPost]
    public async Task<IActionResult> EditPlan(int id, SubscriptionPlan plan)
    {
        if (id != plan.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(plan);

        try
        {
            var existingPlan = await _context.SubscriptionPlans.FindAsync(id);
            if (existingPlan == null)
                return NotFound();

            existingPlan.Name = plan.Name;
            existingPlan.Description = plan.Description;
            existingPlan.MonthlyPrice = plan.MonthlyPrice;
            existingPlan.YearlyPrice = plan.YearlyPrice;
            existingPlan.StripePriceIdMonthly = plan.StripePriceIdMonthly;
            existingPlan.StripePriceIdYearly = plan.StripePriceIdYearly;
            existingPlan.StripeProductId = plan.StripeProductId;
            existingPlan.MaxPopups = plan.MaxPopups;
            existingPlan.MaxPopupViews = plan.MaxPopupViews;
            existingPlan.MaxUsers = plan.MaxUsers;
            existingPlan.HasAdvancedTargeting = plan.HasAdvancedTargeting;
            existingPlan.HasAnalytics = plan.HasAnalytics;
            existingPlan.HasAPIAccess = plan.HasAPIAccess;
            existingPlan.HasPrioritySupport = plan.HasPrioritySupport;
            existingPlan.HasWhiteLabel = plan.HasWhiteLabel;
            existingPlan.IsActive = plan.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Plan updated successfully!";
            return RedirectToAction("Plans");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.SubscriptionPlans.AnyAsync(p => p.Id == id))
                return NotFound();
            throw;
        }
    }

    /// <summary>
    /// Toggle plan active status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TogglePlanStatus(int id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan != null)
        {
            plan.IsActive = !plan.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Plan {(plan.IsActive ? "activated" : "deactivated")} successfully!";
        }
        return RedirectToAction("Plans");
    }

    /// <summary>
    /// Delete subscription plan (only if no tenants are using it)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await _context.SubscriptionPlans
            .Include(p => p.Tenants)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (plan != null)
        {
            if (plan.Tenants.Any())
            {
                TempData["Error"] = "Cannot delete plan with active subscriptions!";
                return RedirectToAction("Plans");
            }

            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Plan deleted successfully!";
        }
        return RedirectToAction("Plans");
    }

    /// <summary>
    /// Manage popup templates available to all tenants
    /// </summary>
    public async Task<IActionResult> Templates()
    {
        var templates = await _context.PopupTemplates.ToListAsync();
        return View(templates);
    }

    /// <summary>
    /// Edit popup template
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditTemplate(int id)
    {
        var template = await _context.PopupTemplates.FindAsync(id);
        if (template == null)
        {
            TempData["Error"] = "Template not found.";
            return RedirectToAction("Templates");
        }
        return View(template);
    }

    [HttpPost]
    public async Task<IActionResult> EditTemplate(PopupTemplate model)
    {
        try
        {
            var template = await _context.PopupTemplates.FindAsync(model.Id);
            if (template == null)
            {
                TempData["Error"] = "Template not found.";
                return RedirectToAction("Templates");
            }
            
            template.Name = model.Name;
            template.Description = model.Description;
            template.Content = model.Content;
            template.ImageUrl = model.ImageUrl;
            template.PreviewImageUrl = model.PreviewImageUrl;
            template.Category = model.Category;
            template.SortOrder = model.SortOrder;
            template.TypeSpecificOptions = model.TypeSpecificOptions;
            template.DefaultTrigger = model.DefaultTrigger;
            template.DefaultFrequency = model.DefaultFrequency;
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Template updated successfully!";
            return RedirectToAction("Templates");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to update template: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Delete popup template
    /// </summary>
    [HttpPost]
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

    /// <summary>
    /// Import default templates from seeder
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ImportDefaultTemplates()
    {
        try
        {
            await DatabaseSeeder.SeedPopupTemplatesAsync(_context);
            TempData["Success"] = "Default templates imported successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to import templates: {ex.Message}";
        }
        
        return RedirectToAction("Templates");
    }

    /// <summary>
    /// View platform statistics and reports
    /// </summary>
    public async Task<IActionResult> Analytics()
    {
        var totalTenants = await _context.Tenants.CountAsync();
        var activeTenants = await _context.Tenants.Where(t => t.IsActive).CountAsync();
        var totalPopups = await _context.Popups.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var plans = await _context.SubscriptionPlans.ToListAsync();
        
        var tenantsByPlan = new Dictionary<string, int>();
        foreach (var plan in plans)
        {
            var count = await _context.Tenants.Where(t => t.SubscriptionPlanId == plan.Id).CountAsync();
            tenantsByPlan[plan.Name] = count;
        }

        var model = new PlatformAnalyticsViewModel
        {
            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            TotalPopups = totalPopups,
            TotalUsers = totalUsers,
            TenantsByPlan = tenantsByPlan,
            AvailablePlans = plans.Count
        };

        return View(model);
    }

    /// <summary>
    /// Sync popup counts for a specific tenant
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SyncPopupCount(int tenantId)
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

    /// <summary>
    /// Reset popup count for a specific tenant
    /// </summary>
    [HttpPost]
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

    /// <summary>
    /// Get platform statistics for dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPlatformStats()
    {
        var stats = new
        {
            totalTenants = await _context.Tenants.CountAsync(),
            activeTenants = await _context.Tenants.Where(t => t.IsActive && t.SubscriptionStatus == "active").CountAsync(),
            totalUsers = await _context.Users.CountAsync(),
            totalPopups = await _context.Popups.CountAsync(),
            activePopups = await _context.Popups.Where(p => p.Status == PopupStatus.Published).CountAsync(),
            activePlans = await _context.SubscriptionPlans.Where(p => p.IsActive).CountAsync()
        };

        return Json(stats);
    }

    /// <summary>
    /// Get revenue statistics by plan
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRevenueStats()
    {
        var plans = await _context.SubscriptionPlans.ToListAsync();
        var revenueByPlan = new Dictionary<string, decimal>();

        foreach (var plan in plans)
        {
            var activeTenants = await _context.Tenants
                .Where(t => t.SubscriptionPlanId == plan.Id && t.SubscriptionStatus == "active")
                .CountAsync();
            
            revenueByPlan[plan.Name] = plan.MonthlyPrice * activeTenants;
        }

        var totalMonthlyRevenue = revenueByPlan.Values.Sum();

        return Json(new
        {
            byPlan = revenueByPlan,
            totalMonthlyRevenue = totalMonthlyRevenue
        });
    }
}

// ViewModels for SuperAdmin
public class PlatformAdminDashboardViewModel
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPopups { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int AvailablePlans { get; set; }
}

public class PlatformAnalyticsViewModel
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalPopups { get; set; }
    public int TotalUsers { get; set; }
    public Dictionary<string, int> TenantsByPlan { get; set; } = new();
    public int AvailablePlans { get; set; }
}
