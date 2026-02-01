namespace Notification_Application.Models;

public class Lead
{
    public int Id { get; set; }
    public int PopupId { get; set; }
    public Popup? Popup { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Basic Information
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    
    // Custom Fields (JSON)
    public string CustomFields { get; set; } = "{}"; // JSON string for flexible fields
    
    // Metadata
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; } // URL where lead was captured
    public string? Referrer { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    
    // GDPR Compliance
    public bool ConsentGiven { get; set; } = false;
    public DateTime? ConsentDate { get; set; }
    public string? ConsentText { get; set; }
    
    // Lead Status
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string? Notes { get; set; }
    public DateTime? LastContactedAt { get; set; }
    
    // View Information - which view was shown when lead captured
    public string? ViewType { get; set; } // e.g., "success", "thank-you", "download"
}

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Converted,
    Unqualified,
    Archived
}
