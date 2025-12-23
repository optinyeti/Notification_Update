namespace Notification_Application.Models;

public class ApiUsage
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public int ResponseStatus { get; set; }
    public long ResponseTimeMs { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ApiKey { get; set; }
}

public class Integration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Configuration stored as JSON
    public string Configuration { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? WebhookUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncAt { get; set; }
}

public enum IntegrationType
{
    Zapier,
    WebHook,
    GoogleAnalytics,
    FacebookPixel,
    Mailchimp,
    HubSpot,
    Salesforce,
    LookerStudio
}