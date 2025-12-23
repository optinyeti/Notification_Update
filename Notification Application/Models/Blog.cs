namespace Notification_Application.Models;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? FeaturedImage { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string AuthorId { get; set; } = string.Empty;
    public User? Author { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // SEO and Analytics
    public int ViewCount { get; set; } = 0;
    public string? CanonicalUrl { get; set; }
    
    // Categories and Tags
    public ICollection<BlogCategory> Categories { get; set; } = new List<BlogCategory>();
    public ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
}

public class BlogCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TenantId { get; set; }
    
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}

public class BlogTag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TenantId { get; set; }
    
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}

public enum BlogPostStatus
{
    Draft,
    Published,
    Scheduled,
    Archived
}