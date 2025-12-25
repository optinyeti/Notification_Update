using Microsoft.AspNetCore.Mvc;
using Notification_Application.Services;
using Notification_Application.Models;

namespace Notification_Application.Controllers;

public class BlogController : Controller
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        // For now, using tenantId = 1 (default). You can modify this based on subdomain routing
        const int tenantId = 1;
        var posts = await _blogService.GetBlogPostsAsync(tenantId, page, 12);
        var categories = await _blogService.GetCategoriesAsync(tenantId);
        
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
