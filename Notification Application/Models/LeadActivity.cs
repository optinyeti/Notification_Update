using System.ComponentModel.DataAnnotations.Schema;

namespace Notification_Application.Models;

public class LeadActivity
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    [Column("ActivityType")]
    public ActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // User who performed the activity (nullable for system activities)
    public string? PerformedByUserId { get; set; }
    public User? PerformedBy { get; set; }
    
    // Activity metadata (JSON)
    public string? Metadata { get; set; } // Store additional data like email subject, call duration, etc.
    
    // Website activity tracking
    public string? PageUrl { get; set; }
    public string? PageTitle { get; set; }
    public int? TimeOnPage { get; set; } // seconds
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? Location { get; set; } // City, State, Country
}

public enum ActivityType
{
    // Manual Activities
    Note,
    Email,
    Call,
    Meeting,
    Task,
    
    // System/Automated Activities
    LeadCreated,
    StatusChanged,
    StageChanged,
    DealCreated,
    DealUpdated,
    FormSubmitted,
    EmailOpened,
    EmailClicked,
    
    // Website Tracking Activities (from pixel)
    PageView,
    PopupView,
    PopupInteraction,
    ButtonClick,
    VideoWatch,
    FileDownload,
    ProductView,
    CartAbandonment,
    Purchase,
    
    // Custom
    Custom
}
