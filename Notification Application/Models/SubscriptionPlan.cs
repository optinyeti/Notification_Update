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
    
    // Plan limits (-1 = unlimited, 0 = no access)
    public int MaxPopups { get; set; }
    public int MaxPopupViews { get; set; }
    public int MaxForms { get; set; }
    public int MaxUsers { get; set; }
    public int MaxWebsites { get; set; } = 1;
    public int MaxLandingPages { get; set; } = 0;
    
    // AI Features
    public int MaxAIRequests { get; set; } = 0; // Monthly AI request limit (0 = no access, -1 = unlimited)
    public bool HasAIAccess => MaxAIRequests != 0;
    
    // CRM Features
    public int MaxContacts { get; set; } = 100; // Max leads/contacts
    public int MaxPipelines { get; set; } = 1; // Max sales pipelines
    
    // Form Features
    public int MaxFormSubmissions { get; set; } = 50; // Monthly form submission limit
    
    // Feature Flags
    public bool HasAdvancedTargeting { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasWhiteLabel { get; set; }
    public bool HasLandingPageBuilder { get; set; } = false;
    public bool CanRemoveBranding { get; set; } = false;
    public bool HasRoleBasedAccess { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}