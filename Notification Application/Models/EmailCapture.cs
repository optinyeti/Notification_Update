namespace Notification_Application.Models;

public class EmailCapture
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    
    public int PopupId { get; set; }
    public Popup? Popup { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; }
    
    // GDPR compliance
    public bool ConsentGiven { get; set; } = false;
    public DateTime? ConsentDate { get; set; }
    public string? ConsentText { get; set; }
}