namespace Notification_Application.Models;

public class Lead
{
    public int Id { get; set; }
    public int? PopupId { get; set; }
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
    public LeadDisposition Disposition { get; set; } = LeadDisposition.None;
    public string? Notes { get; set; }
    public DateTime? LastContactedAt { get; set; }
    
    // Deal/Value Information
    public decimal? PotentialValue { get; set; }
    public int? AssignedDealId { get; set; }
    public Deal? AssignedDeal { get; set; }
    
    // View Information - which view was shown when lead captured
    public string? ViewType { get; set; } // e.g., "success", "thank-you", "download"
    
    // CRM Pipeline
    public int? PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }
    
    public int? StageId { get; set; }
    public PipelineStage? Stage { get; set; }
    
    public DateTime? StageEnteredAt { get; set; }
    
    // Lead Scoring
    public int Score { get; set; } = 0;
    public DateTime? LastScoreUpdate { get; set; }
    
    // Lead Owner (assigned sales rep)
    public string? OwnerId { get; set; }
    public User? Owner { get; set; }
    
    // Lead Source Details
    public string? LeadSource { get; set; } // e.g., "Website", "Social Media", "Referral"
    public string? LeadSourceDetail { get; set; }
    
    // Engagement Metrics (populated from pixel tracking)
    public int PageViews { get; set; } = 0;
    public int TotalVisits { get; set; } = 0;
    public DateTime? FirstVisitAt { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public int? AverageTimeOnSite { get; set; } // seconds
    public int EmailOpens { get; set; } = 0;
    public int EmailClicks { get; set; } = 0;
    
    // Custom Properties
    public string? Industry { get; set; }
    public string? JobTitle { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    
    // Navigation Properties
    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
    public ICollection<LeadStageHistory> StageHistory { get; set; } = new List<LeadStageHistory>();
    public ICollection<LeadScore> ScoreHistory { get; set; } = new List<LeadScore>();
    public ICollection<LeadTag> Tags { get; set; } = new List<LeadTag>();
    public ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
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

public enum LeadDisposition
{
    None,
    Interested,
    NotInterested,
    CallBack,
    LeftVoicemail,
    NoAnswer,
    WrongNumber,
    DoNotContact,
    Nurturing,
    ReadyToBuy,
    NeedsFollowUp,
    MeetingScheduled,
    ProposalSent,
    NegotiatingPrice,
    ContractSent,
    Closed
}
