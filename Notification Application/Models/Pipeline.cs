namespace Notification_Application.Models;

public class Pipeline
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public int Order { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<PipelineStage> Stages { get; set; } = new List<PipelineStage>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class PipelineStage
{
    public int Id { get; set; }
    public int PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#667eea"; // Hex color for visual representation
    public int Order { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Stage configuration
    public bool IsWonStage { get; set; } = false; // Marks as successful conversion
    public bool IsLostStage { get; set; } = false; // Marks as lost opportunity
    public int? ExpectedDaysInStage { get; set; } // For tracking if lead is stuck
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class LeadStageHistory
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public int? FromStageId { get; set; }
    public PipelineStage? FromStage { get; set; }
    
    public int ToStageId { get; set; }
    public PipelineStage? ToStage { get; set; }
    
    public DateTime MovedAt { get; set; } = DateTime.UtcNow;
    public string? MovedByUserId { get; set; }
    public User? MovedBy { get; set; }
    
    public string? Notes { get; set; }
    public int DaysInPreviousStage { get; set; } = 0;
}
