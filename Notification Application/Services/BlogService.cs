using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;

    public BlogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPost?> GetBlogPostAsync(int id)
    {
        return await _context.BlogPosts
            .Include(bp => bp.Author)
            .Include(bp => bp.Categories)
            .Include(bp => bp.Tags)
            .FirstOrDefaultAsync(bp => bp.Id == id);
    }

    public async Task<BlogPost?> GetBlogPostBySlugAsync(string slug)
    {
        var post = await _context.BlogPosts
            .Include(bp => bp.Author)
            .Include(bp => bp.Categories)
            .Include(bp => bp.Tags)
            .FirstOrDefaultAsync(bp => bp.Slug == slug);

        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return post;
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int tenantId, int page = 1, int pageSize = 10)
    {
        return await _context.BlogPosts
            .Where(bp => bp.TenantId == tenantId && bp.Status == BlogPostStatus.Published)
            .Include(bp => bp.Author)
            .Include(bp => bp.Categories)
            .Include(bp => bp.Tags)
            .OrderByDescending(bp => bp.PublishedAt ?? bp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
    {
        blogPost.Slug = GenerateSlug(blogPost.Title);
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost)
    {
        blogPost.UpdatedAt = DateTime.UtcNow;
        _context.BlogPosts.Update(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task DeleteBlogPostAsync(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost != null)
        {
            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BlogCategory>> GetCategoriesAsync(int tenantId)
    {
        return await _context.BlogCategories
            .Where(bc => bc.TenantId == tenantId)
            .OrderBy(bc => bc.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlogTag>> GetTagsAsync(int tenantId)
    {
        return await _context.BlogTags
            .Where(bt => bt.TenantId == tenantId)
            .OrderBy(bt => bt.Name)
            .ToListAsync();
    }

    private string GenerateSlug(string title)
    {
        return title.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "");
    }
}