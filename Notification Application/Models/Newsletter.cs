namespace Notification_Application.Models;

public class Newsletter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NewsletterStatus Status { get; set; } = NewsletterStatus.Draft;
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    
    // Analytics
    public int TotalSent { get; set; } = 0;
    public int Opens { get; set; } = 0;
    public int Clicks { get; set; } = 0;
    public int Bounces { get; set; } = 0;
    public int Unsubscribes { get; set; } = 0;
    
    // Recipients
    public ICollection<NewsletterRecipient> Recipients { get; set; } = new List<NewsletterRecipient>();
}

public class NewsletterRecipient
{
    public int Id { get; set; }
    public int NewsletterId { get; set; }
    public Newsletter? Newsletter { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public DateTime? SentAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public bool Bounced { get; set; } = false;
    public bool Unsubscribed { get; set; } = false;
}

public enum NewsletterStatus
{
    Draft,
    Scheduled,
    Sending,
    Sent,
    Cancelled
}