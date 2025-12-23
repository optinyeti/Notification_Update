using Notification_Application.Models;
using System.ComponentModel.DataAnnotations;

namespace Notification_Application.Models;

public class DashboardViewModel
{
    public int TotalPopups { get; set; }
    public int ActivePopups { get; set; }
    public object? AnalyticsSummary { get; set; }
    public List<Popup> RecentPopups { get; set; } = new();
}

public class ContactFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class AdminDashboardViewModel
{
    public object? AnalyticsSummary { get; set; }
    public int OpenTickets { get; set; }
    public int TotalTickets { get; set; }
}

public class TenantSettingsViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Public App URL")]
    [Url]
    public string? PublicHostUrl { get; set; }

    // Stripe Configuration
    [Display(Name = "Stripe Secret Key")]
    public string? StripeSecretKey { get; set; }

    [Display(Name = "Stripe Publishable Key")]
    public string? StripePublishableKey { get; set; }

    [Display(Name = "Stripe Webhook Secret")]
    public string? StripeWebhookSecret { get; set; }
}

public class AnalyticsViewModel
{
    public object? Summary { get; set; }
    public List<PopupAnalytics> PopupAnalytics { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CreateSupportTicketViewModel
{
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public TicketCategory Category { get; set; } = TicketCategory.General;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}

public class PopupCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public PopupType Type { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(300)]
    public string? Subtitle { get; set; }

    [StringLength(100)]
    public string? CallToAction { get; set; }

    public string Content { get; set; } = "{}";
    public string TargetingRules { get; set; } = "{}";
    public string TypeSpecificOptions { get; set; } = "{}";
    public PopupTrigger Trigger { get; set; } = PopupTrigger.OnPageLoad;

    [Range(0, 60000)]
    public int DelayMs { get; set; } = 0;

    public PopupFrequency Frequency { get; set; } = PopupFrequency.EveryVisit;
    public bool ShowOnMobile { get; set; } = true;
    public bool ShowOnDesktop { get; set; } = true;

    // Type-specific options
    [Url]
    public string? ImageUrl { get; set; }
    [Url]
    public string? VideoUrl { get; set; }
    public string? CouponCode { get; set; }
}

public class PopupEditViewModel : PopupCreateViewModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public PopupStatus Status { get; set; }
}

public class PopupDesignModel
{
    public string? Content { get; set; }
    public string? TargetingRules { get; set; }
    public string? Trigger { get; set; }
    public string? Frequency { get; set; }
    public int? DelayMs { get; set; }
    public bool? ShowOnMobile { get; set; }
    public bool? ShowOnDesktop { get; set; }
}

public class PopupComponent
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public object Style { get; set; } = new();
    public object Properties { get; set; } = new();
}

public class PopupStyle
{
    public string BackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#333333";
    public string FontFamily { get; set; } = "Arial, sans-serif";
    public int BorderRadius { get; set; } = 8;
    public string BorderColor { get; set; } = "#e0e0e0";
    public int BorderWidth { get; set; } = 1;
}

public class PopupAnalyticsViewModel
{
    public Popup Popup { get; set; } = new();
    public List<PopupAnalytics> Analytics { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Account ViewModels
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CompanyName { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    // Read-only properties for display
    public string CompanyName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}