using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IBlogService _blogService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminController(
        ITenantService tenantService,
        IAnalyticsService analyticsService,
        ISupportService supportService,
        IPopupTemplateService templateService,
        IBlogService blogService,
        UserManager<User> userManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _tenantService = tenantService;
        _analyticsService = analyticsService;
        _supportService = supportService;
        _templateService = templateService;
        _blogService = blogService;
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
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

        // Auto-populate PublicHostUrl if not set
        if (string.IsNullOrWhiteSpace(tenant.PublicHostUrl))
        {
            var configuredHost = _configuration["AppSettings:ProductionUrl"];
            if (string.IsNullOrWhiteSpace(configuredHost))
            {
                configuredHost = _configuration["AppSettings:BaseUrl"];
            }
            if (string.IsNullOrWhiteSpace(configuredHost))
            {
                configuredHost = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            }
            tenant.PublicHostUrl = configuredHost;
            await _tenantService.UpdateTenantAsync(tenant);
        }

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

    // ============ BLOG MANAGEMENT ============

    public async Task<IActionResult> Blog()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var posts = await _context.BlogPosts
            .Where(bp => bp.TenantId == user.TenantId)
            .Include(bp => bp.Author)
            .Include(bp => bp.Categories)
            .Include(bp => bp.Tags)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    public async Task<IActionResult> CreateBlogPost()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var categories = await _blogService.GetCategoriesAsync(user.TenantId);
        var tags = await _blogService.GetTagsAsync(user.TenantId);

        ViewBag.Categories = categories;
        ViewBag.Tags = tags;

        return View(new BlogPost());
    }

    [HttpPost]
    public async Task<IActionResult> CreateBlogPost(BlogPost model, int[] categoryIds, int[] tagIds)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            var categories = await _blogService.GetCategoriesAsync(user.TenantId);
            var tags = await _blogService.GetTagsAsync(user.TenantId);
            ViewBag.Categories = categories;
            ViewBag.Tags = tags;
            return View(model);
        }

        model.TenantId = user.TenantId;
        model.AuthorId = user.Id;
        model.CreatedAt = DateTime.UtcNow;

        if (model.Status == BlogPostStatus.Published && model.PublishedAt == null)
        {
            model.PublishedAt = DateTime.UtcNow;
        }

        // Add categories and tags
        if (categoryIds != null && categoryIds.Length > 0)
        {
            var selectedCategories = await _context.BlogCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();
            model.Categories = selectedCategories;
        }

        if (tagIds != null && tagIds.Length > 0)
        {
            var selectedTags = await _context.BlogTags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
            model.Tags = selectedTags;
        }

        await _blogService.CreateBlogPostAsync(model);
        TempData["Success"] = "Blog post created successfully!";
        
        return RedirectToAction("Blog");
    }

    public async Task<IActionResult> EditBlogPost(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = await _blogService.GetBlogPostAsync(id);
        if (post == null || post.TenantId != user.TenantId)
            return NotFound();

        var categories = await _blogService.GetCategoriesAsync(user.TenantId);
        var tags = await _blogService.GetTagsAsync(user.TenantId);

        ViewBag.Categories = categories;
        ViewBag.Tags = tags;
        ViewBag.SelectedCategoryIds = post.Categories.Select(c => c.Id).ToList();
        ViewBag.SelectedTagIds = post.Tags.Select(t => t.Id).ToList();

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> EditBlogPost(BlogPost model, int[] categoryIds, int[] tagIds)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existingPost = await _blogService.GetBlogPostAsync(model.Id);
        if (existingPost == null || existingPost.TenantId != user.TenantId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var categories = await _blogService.GetCategoriesAsync(user.TenantId);
            var tags = await _blogService.GetTagsAsync(user.TenantId);
            ViewBag.Categories = categories;
            ViewBag.Tags = tags;
            return View(model);
        }

        existingPost.Title = model.Title;
        existingPost.Content = model.Content;
        existingPost.Excerpt = model.Excerpt;
        existingPost.FeaturedImage = model.FeaturedImage;
        existingPost.MetaDescription = model.MetaDescription;
        existingPost.MetaKeywords = model.MetaKeywords;
        existingPost.Status = model.Status;
        existingPost.UpdatedAt = DateTime.UtcNow;

        if (model.Status == BlogPostStatus.Published && existingPost.PublishedAt == null)
        {
            existingPost.PublishedAt = DateTime.UtcNow;
        }

        // Update categories
        existingPost.Categories.Clear();
        if (categoryIds != null && categoryIds.Length > 0)
        {
            var selectedCategories = await _context.BlogCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();
            existingPost.Categories = selectedCategories;
        }

        // Update tags
        existingPost.Tags.Clear();
        if (tagIds != null && tagIds.Length > 0)
        {
            var selectedTags = await _context.BlogTags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
            existingPost.Tags = selectedTags;
        }

        await _blogService.UpdateBlogPostAsync(existingPost);
        TempData["Success"] = "Blog post updated successfully!";
        
        return RedirectToAction("Blog");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBlogPost(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = await _blogService.GetBlogPostAsync(id);
        if (post == null || post.TenantId != user.TenantId)
            return NotFound();

        await _blogService.DeleteBlogPostAsync(id);
        TempData["Success"] = "Blog post deleted successfully!";
        
        return RedirectToAction("Blog");
    }

    // Blog Categories Management
    public async Task<IActionResult> BlogCategories()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var categories = await _blogService.GetCategoriesAsync(user.TenantId);
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(string name, string description)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var category = new BlogCategory
        {
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
            Description = description,
            TenantId = user.TenantId
        };

        _context.BlogCategories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Category created successfully!";
        return RedirectToAction("BlogCategories");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var category = await _context.BlogCategories.FindAsync(id);
        if (category != null && category.TenantId == user.TenantId)
        {
            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted successfully!";
        }

        return RedirectToAction("BlogCategories");
    }
}
