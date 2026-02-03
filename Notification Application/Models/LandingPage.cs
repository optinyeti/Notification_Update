namespace Notification_Application.Models;

/// <summary>
/// Landing pages created by the tenant using the landing page builder.
/// </summary>
public class LandingPage
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    /// <summary>
    /// Name/title of the landing page for internal reference.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// SEO-friendly slug for the URL (e.g., "summer-sale-2026").
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom domain if user has connected their domain via A record.
    /// Example: "promo.example.com"
    /// </summary>
    public string? CustomDomain { get; set; }
    
    /// <summary>
    /// Full landing page HTML content.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS styles for the landing page.
    /// </summary>
    public string? CssContent { get; set; }
    
    /// <summary>
    /// JSON configuration for the page builder state.
    /// </summary>
    public string? BuilderConfig { get; set; }
    
    /// <summary>
    /// Meta title for SEO.
    /// </summary>
    public string? MetaTitle { get; set; }
    
    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }
    
    /// <summary>
    /// Open Graph image URL for social sharing.
    /// </summary>
    public string? OgImageUrl { get; set; }
    
    /// <summary>
    /// Whether the landing page is published and accessible.
    /// </summary>
    public bool IsPublished { get; set; } = false;
    
    /// <summary>
    /// Number of views the landing page has received.
    /// </summary>
    public int ViewCount { get; set; } = 0;
    
    /// <summary>
    /// Number of conversions (form submissions, clicks, etc.).
    /// </summary>
    public int ConversionCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// User ID who created this landing page.
    /// </summary>
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
}
