using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;

namespace Notification_Application.Controllers;

/// <summary>
/// AdminController handles tenant-level administration tasks like managing team members,
/// settings, blog posts, and templates for a specific tenant.
/// Platform-level admin operations are in SuperAdminController.
/// User-specific dashboards are in UserDashboardController.
/// </summary>
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly IBillingService _billingService;
    private readonly IBlogService _blogService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminController(
        ITenantService tenantService,
        IBillingService billingService,
        IBlogService blogService,
        UserManager<User> userManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _tenantService = tenantService;
        _billingService = billingService;
        _blogService = blogService;
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Tenant admin dashboard - Shows team members, settings, and team analytics
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        var teamMembers = await _context.Users.Where(u => u.TenantId == user.TenantId).ToListAsync();
        var usage = await _billingService.GetUsageStatsAsync(user.TenantId);

        var model = new AdminDashboardViewModel
        {
            Tenant = tenant,
            TeamMemberCount = teamMembers.Count,
            UsageStats = usage,
            ActiveTeamMembers = teamMembers.Count(u => u.IsActive)
        };

        return View(model);
    }

    /// <summary>
    /// Manage team members for this tenant
    /// </summary>
    public async Task<IActionResult> Users()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var tenant = await _tenantService.GetTenantAsync(user.TenantId);
        if (tenant == null) return NotFound();

        var users = await _context.Users
            .Where(u => u.TenantId == user.TenantId)
            .OrderBy(u => u.FirstName)
            .ToListAsync();

        // Get roles from ASP.NET Identity and sync with database
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            if (roles.Count > 0)
            {
                // Update database Role to match ASP.NET Identity role
                var identityRole = roles.FirstOrDefault();
                var userRole = identityRole switch
                {
                    "SuperAdmin" => UserRole.SuperAdmin,
                    "Admin" => UserRole.Admin,
                    "User" => UserRole.User,
                    _ => UserRole.User
                };

                if (u.Role != userRole)
                {
                    u.Role = userRole;
                }
            }
        }

        // Save any role updates
        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }
        
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var userToEdit = await _userManager.FindByIdAsync(id);
        if (userToEdit == null || userToEdit.TenantId != currentUser.TenantId)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Users");
        }

        return View(userToEdit);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, string firstName, string lastName, string email, UserRole role, bool isActive)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var userToEdit = await _userManager.FindByIdAsync(id);
        if (userToEdit == null || userToEdit.TenantId != currentUser.TenantId)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Users");
        }

        userToEdit.FirstName = firstName;
        userToEdit.LastName = lastName;
        userToEdit.Email = email;
        userToEdit.NormalizedEmail = email.ToUpper();
        userToEdit.UserName = email;
        userToEdit.NormalizedUserName = email.ToUpper();
        userToEdit.Role = role;
        userToEdit.IsActive = isActive;

        var result = await _userManager.UpdateAsync(userToEdit);
        if (result.Succeeded)
        {
            // Update ASP.NET Identity roles
            var currentRoles = await _userManager.GetRolesAsync(userToEdit);
            var newRoleName = role switch
            {
                UserRole.SuperAdmin => "SuperAdmin",
                UserRole.Admin => "Admin",
                UserRole.User => "User",
                _ => "User"
            };

            // Remove all current roles and add the new one
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(userToEdit, currentRoles);
            }
            await _userManager.AddToRoleAsync(userToEdit, newRoleName);

            TempData["Success"] = "User updated successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to update user.";
        }

        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> UserActivity(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var userToView = await _userManager.FindByIdAsync(id);
        if (userToView == null || userToView.TenantId != currentUser.TenantId)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Users");
        }

        // Get user's activity data
        var popups = await _context.Popups
            .Where(p => p.TenantId == currentUser.TenantId)
            .Include(p => p.Analytics)
            .ToListAsync();

        ViewBag.User = userToView;
        return View(popups);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var userToToggle = await _userManager.FindByIdAsync(id);
        if (userToToggle == null || userToToggle.TenantId != currentUser.TenantId)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Users");
        }

        // Don't allow deactivating yourself
        if (userToToggle.Id == currentUser.Id)
        {
            TempData["Error"] = "You cannot deactivate yourself.";
            return RedirectToAction("Users");
        }

        userToToggle.IsActive = !userToToggle.IsActive;
        var result = await _userManager.UpdateAsync(userToToggle);

        if (result.Succeeded)
        {
            TempData["Success"] = $"User {(userToToggle.IsActive ? "activated" : "deactivated")} successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to update user status.";
        }

        return RedirectToAction("Users");
    }

    public async Task<IActionResult> Analytics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var analytics = await _context.PopupAnalytics
            .Include(a => a.Popup)
            .Where(a => a.Popup.TenantId == user.TenantId)
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .ToListAsync();

        var model = new AnalyticsViewModel
        {
            PopupAnalytics = analytics,
            StartDate = startDate,
            EndDate = endDate
        };

        return View(model);
    }

    /// <summary>
    /// Manage tenant settings like name, domain, and host URL
    /// </summary>
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
