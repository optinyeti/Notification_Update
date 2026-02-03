using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.RegularExpressions;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User")]
public class LandingPageController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<LandingPageController> _logger;

    public LandingPageController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<LandingPageController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: LandingPage
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

        if (tenant == null) return NotFound();

        // Check if user has access to landing page builder
        if (!tenant.SubscriptionPlan!.HasLandingPageBuilder)
        {
            return RedirectToAction("Upgrade", "Subscription");
        }

        var landingPages = await _context.LandingPages
            .Where(lp => lp.TenantId == user.TenantId)
            .OrderByDescending(lp => lp.CreatedAt)
            .ToListAsync();

        ViewBag.MaxLandingPages = tenant.SubscriptionPlan.MaxLandingPages;
        ViewBag.CurrentCount = landingPages.Count;

        return View(landingPages);
    }

    // GET: LandingPage/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

        if (tenant == null) return NotFound();

        // Check plan limits
        if (!tenant.SubscriptionPlan!.HasLandingPageBuilder)
        {
            return RedirectToAction("Upgrade", "Subscription");
        }

        var currentCount = await _context.LandingPages
            .CountAsync(lp => lp.TenantId == user.TenantId);

        if (currentCount >= tenant.SubscriptionPlan.MaxLandingPages && tenant.SubscriptionPlan.MaxLandingPages < 9999)
        {
            TempData["Error"] = $"You've reached your plan limit of {tenant.SubscriptionPlan.MaxLandingPages} landing pages.";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    // POST: LandingPage/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLandingPageRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

        if (tenant == null) return NotFound();

        // Validate plan access
        if (!tenant.SubscriptionPlan!.HasLandingPageBuilder)
        {
            return Json(new { success = false, message = "Landing page builder not available on your plan" });
        }

        // Check limits
        var currentCount = await _context.LandingPages
            .CountAsync(lp => lp.TenantId == user.TenantId);

        if (currentCount >= tenant.SubscriptionPlan.MaxLandingPages && tenant.SubscriptionPlan.MaxLandingPages < 9999)
        {
            return Json(new { success = false, message = "Landing page limit reached" });
        }

        // Generate slug from name
        var slug = GenerateSlug(request.Name);

        // Check if slug exists
        var slugExists = await _context.LandingPages
            .AnyAsync(lp => lp.TenantId == user.TenantId && lp.Slug == slug);

        if (slugExists)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var landingPage = new LandingPage
        {
            TenantId = user.TenantId,
            Name = request.Name,
            Slug = slug,
            HtmlContent = request.HtmlContent ?? GetDefaultHtmlTemplate(),
            CssContent = request.CssContent,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.LandingPages.Add(landingPage);
        await _context.SaveChangesAsync();

        return Json(new { success = true, id = landingPage.Id, slug = landingPage.Slug });
    }

    // GET: LandingPage/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var landingPage = await _context.LandingPages
            .FirstOrDefaultAsync(lp => lp.Id == id && lp.TenantId == user.TenantId);

        if (landingPage == null) return NotFound();

        return View(landingPage);
    }

    // POST: LandingPage/Save
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveLandingPageRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var landingPage = await _context.LandingPages
            .FirstOrDefaultAsync(lp => lp.Id == request.Id && lp.TenantId == user.TenantId);

        if (landingPage == null) return NotFound();

        landingPage.Name = request.Name ?? landingPage.Name;
        landingPage.HtmlContent = request.HtmlContent ?? landingPage.HtmlContent;
        landingPage.CssContent = request.CssContent;
        landingPage.BuilderConfig = request.BuilderConfig;
        landingPage.MetaTitle = request.MetaTitle;
        landingPage.MetaDescription = request.MetaDescription;
        landingPage.OgImageUrl = request.OgImageUrl;
        landingPage.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST: LandingPage/Publish/5
    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var landingPage = await _context.LandingPages
            .FirstOrDefaultAsync(lp => lp.Id == id && lp.TenantId == user.TenantId);

        if (landingPage == null) return NotFound();

        landingPage.IsPublished = !landingPage.IsPublished;
        landingPage.PublishedAt = landingPage.IsPublished ? DateTime.UtcNow : null;
        landingPage.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Json(new { success = true, isPublished = landingPage.IsPublished });
    }

    // DELETE: LandingPage/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var landingPage = await _context.LandingPages
            .FirstOrDefaultAsync(lp => lp.Id == id && lp.TenantId == user.TenantId);

        if (landingPage == null) return NotFound();

        _context.LandingPages.Remove(landingPage);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // GET: /p/{slug} - Public landing page view
    [AllowAnonymous]
    [Route("/p/{slug}")]
    public async Task<IActionResult> View(string slug, int? tenantId)
    {
        LandingPage? landingPage = null;

        if (tenantId.HasValue)
        {
            landingPage = await _context.LandingPages
                .Include(lp => lp.Tenant)
                .FirstOrDefaultAsync(lp => lp.Slug == slug && lp.TenantId == tenantId && lp.IsPublished);
        }
        else
        {
            landingPage = await _context.LandingPages
                .Include(lp => lp.Tenant)
                .FirstOrDefaultAsync(lp => lp.Slug == slug && lp.IsPublished);
        }

        if (landingPage == null) return NotFound();

        // Increment view count
        landingPage.ViewCount++;
        await _context.SaveChangesAsync();

        ViewBag.MetaTitle = landingPage.MetaTitle ?? landingPage.Name;
        ViewBag.MetaDescription = landingPage.MetaDescription ?? "";
        ViewBag.OgImageUrl = landingPage.OgImageUrl;
        ViewBag.GTM = landingPage.Tenant?.GoogleTagManagerId;
        ViewBag.GA4 = landingPage.Tenant?.GoogleAnalytics4Id;

        return View("PublicView", landingPage);
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private string GetDefaultHtmlTemplate()
    {
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Landing Page</title>
</head>
<body>
    <div style=""max-width: 1200px; margin: 0 auto; padding: 60px 20px; text-align: center;"">
        <h1 style=""font-size: 48px; margin-bottom: 20px;"">Welcome!</h1>
        <p style=""font-size: 20px; color: #666; margin-bottom: 40px;"">Start building your landing page.</p>
        <button style=""background: #2563eb; color: white; padding: 15px 40px; border: none; border-radius: 8px; font-size: 18px; cursor: pointer;"">Get Started</button>
    </div>
</body>
</html>";
    }
}

// Request Models
public class CreateLandingPageRequest
{
    public string Name { get; set; } = string.Empty;
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class SaveLandingPageRequest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? BuilderConfig { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
}
