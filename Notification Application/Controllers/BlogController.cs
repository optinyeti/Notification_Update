using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Services;
using Notification_Application.Models;
using Notification_Application.Data;

namespace Notification_Application.Controllers;

public class BlogController : Controller
{
    private readonly IBlogService _blogService;
    private readonly ApplicationDbContext _context;

    public BlogController(IBlogService blogService, ApplicationDbContext context)
    {
        _blogService = blogService;
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        // Get all published posts from all tenants
        var posts = await _context.BlogPosts
            .Where(bp => bp.Status == BlogPostStatus.Published)
            .Include(bp => bp.Author)
            .Include(bp => bp.Categories)
            .Include(bp => bp.Tags)
            .OrderByDescending(bp => bp.PublishedAt ?? bp.CreatedAt)
            .Skip((page - 1) * 12)
            .Take(12)
            .ToListAsync();
        
        var categories = await _context.BlogCategories
            .OrderBy(bc => bc.Name)
            .ToListAsync();
        
        ViewBag.Categories = categories;
        ViewBag.CurrentPage = page;
        
        return View(posts);
    }

    public async Task<IActionResult> Post(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var post = await _blogService.GetBlogPostBySlugAsync(slug);
        
        if (post == null || post.Status != BlogPostStatus.Published)
            return NotFound();

        var relatedPosts = await _blogService.GetBlogPostsAsync(post.TenantId, 1, 3);
        ViewBag.RelatedPosts = relatedPosts;

        return View(post);
    }

    public async Task<IActionResult> Category(string slug, int page = 1)
    {
        // TODO: Implement category filtering
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Tag(string slug, int page = 1)
    {
        // TODO: Implement tag filtering
        return RedirectToAction("Index");
    }
}
