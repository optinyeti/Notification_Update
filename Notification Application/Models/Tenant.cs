namespace Notification_Application.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Public host override for embeds and API URLs
    public string? PublicHostUrl { get; set; }
    
    // Usage tracking
    public int PopupCount { get; set; } = 0;
    public int MonthlyPopupViews { get; set; } = 0;
    public DateTime? LastUsageReset { get; set; }
    
    // Stripe Integration
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionStatus { get; set; } // active, canceled, past_due, etc.
    
    // Stripe Configuration (tenant-specific keys)
    public string? StripeSecretKey { get; set; }
    public string? StripePublishableKey { get; set; }
    public string? StripeWebhookSecret { get; set; }
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Popup> Popups { get; set; } = new List<Popup>();
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public ICollection<Newsletter> Newsletters { get; set; } = new List<Newsletter>();
}