using System.ComponentModel.DataAnnotations;

namespace Notification_Application.Models;

public class WebsitePage
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;
    
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public bool IsPublished { get; set; } = false;
    public bool IsHomepage { get; set; } = false;
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    public string? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}
