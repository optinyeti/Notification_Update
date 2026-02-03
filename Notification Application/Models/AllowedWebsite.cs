namespace Notification_Application.Models;

/// <summary>
/// Tracks websites where the tenant is allowed to use popups and tracking pixels.
/// Limited by subscription plan's MaxWebsites value.
/// </summary>
public class AllowedWebsite
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    /// <summary>
    /// The domain or URL pattern where popups/tracking are allowed.
    /// Examples: "example.com", "www.example.com", "shop.example.com"
    /// </summary>
    public string Domain { get; set; } = string.Empty;
    
    /// <summary>
    /// Full URL with protocol for display purposes.
    /// Example: "https://example.com"
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique tracking key for this specific website.
    /// Can be used instead of tenant-wide key for per-site analytics.
    /// </summary>
    public string TrackingKey { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Whether this website is currently active for tracking.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the website was added to the account.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional notes about this website.
    /// </summary>
    public string? Notes { get; set; }
}
