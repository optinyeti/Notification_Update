using System.ComponentModel.DataAnnotations;

namespace Notification_Application.Models;

public class Popup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PopupType Type { get; set; }
    public PopupStatus Status { get; set; } = PopupStatus.Draft;
    
    // Content and design
    public string Content { get; set; } = string.Empty; // JSON content for WYSIWYG editor
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? CallToAction { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    // Targeting and behavior
    public string TargetingRules { get; set; } = string.Empty; // JSON targeting configuration
    public PopupTrigger Trigger { get; set; } = PopupTrigger.OnPageLoad;
    public int DelayMs { get; set; } = 0;
    public PopupFrequency Frequency { get; set; } = PopupFrequency.EveryVisit;
    public bool ShowOnMobile { get; set; } = true;
    public bool ShowOnDesktop { get; set; } = true;
    
    // Multi-tenant
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    // Analytics
    public int Views { get; set; } = 0;
    public int Clicks { get; set; } = 0;
    public int Conversions { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public ICollection<PopupAnalytics> Analytics { get; set; } = new List<PopupAnalytics>();
    public ICollection<EmailCapture> EmailCaptures { get; set; } = new List<EmailCapture>();
}

public enum PopupType
{
    Message,
    EmailCollector,
    Advertising,
    Lightbox,
    Inline,
    SpinWheel,
    VideoPopup,
    Coupon
}

public enum PopupStatus
{
    Draft,
    Published,
    Paused,
    Archived
}

public enum PopupTrigger
{
    OnPageLoad,
    OnExitIntent,
    OnScroll,
    OnTimeDelay,
    OnClick,
    OnIdle
}

public enum PopupFrequency
{
    EveryVisit,
    OncePerSession,
    OncePerDay,
    OncePerWeek,
    OncePerMonth,
    OnceEver
}