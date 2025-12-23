namespace Notification_Application.Models;

public class SupportTicket
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketCategory Category { get; set; } = TicketCategory.General;
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    public string? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // Navigation properties
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}

public class TicketMessage
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }
    
    public string Message { get; set; } = string.Empty;
    public bool IsFromSupport { get; set; } = false;
    
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Attachments
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
}

public enum TicketStatus
{
    Open,
    InProgress,
    Waiting,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    General,
    Technical,
    Billing,
    FeatureRequest,
    BugReport
}