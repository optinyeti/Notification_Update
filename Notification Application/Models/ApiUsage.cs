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
    
    // AI-specific fields
    public string? ApiType { get; set; } // e.g., "OpenAI", "Claude", etc.
    public int? TokensUsed { get; set; }
    public decimal? Cost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Integration model moved to Integration.cs