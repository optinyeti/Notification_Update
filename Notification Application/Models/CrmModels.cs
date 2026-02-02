namespace Notification_Application.Models;

public class LeadScore
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public int Score { get; set; } = 0;
    public string? Reason { get; set; } // Why score changed
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class LeadTag
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#667eea";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class LeadCustomField
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; } = 0;
    
    // For dropdown/select fields
    public string? Options { get; set; } // JSON array of options
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum FieldType
{
    Text,
    Email,
    Phone,
    Number,
    Date,
    Dropdown,
    MultiSelect,
    Checkbox,
    Textarea,
    Url
}

public class CrmTask
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public CrmTaskStatus Status { get; set; } = CrmTaskStatus.Pending;
    
    public string? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }
    
    public string CreatedByUserId { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum CrmTaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public class Deal
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Probability { get; set; } = 50; // 0-100
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    
    public DealStatus Status { get; set; } = DealStatus.Open;
    
    public string? Notes { get; set; }
    
    public string? OwnerId { get; set; }
    public User? Owner { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum DealStatus
{
    Open,
    Won,
    Lost,
    Abandoned
}
