namespace Notification_Application.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    
    // Stripe Integration
    public string? StripePriceIdMonthly { get; set; }
    public string? StripePriceIdYearly { get; set; }
    public string? StripeProductId { get; set; }
    
    // Plan limits
    public int MaxPopups { get; set; }
    public int MaxPopupViews { get; set; }
    public int MaxUsers { get; set; }
    public bool HasAdvancedTargeting { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasWhiteLabel { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}