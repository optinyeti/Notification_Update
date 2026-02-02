namespace Notification_Application.Models;

public class WebsiteForm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Form Settings
    public FormType FormType { get; set; } = FormType.ContactForm;
    public FormStyle Style { get; set; } = FormStyle.Standard;
    public string Theme { get; set; } = "light"; // light, dark, custom
    
    // Appearance
    public string? BackgroundColor { get; set; } = "#ffffff";
    public string? TextColor { get; set; } = "#333333";
    public string? ButtonColor { get; set; } = "#667eea";
    public string? ButtonTextColor { get; set; } = "#ffffff";
    public string? BorderColor { get; set; } = "#e5e7eb";
    public int BorderRadius { get; set; } = 8;
    public string? FontFamily { get; set; } = "Inter, sans-serif";
    public int FontSize { get; set; } = 14;
    public string? CustomCss { get; set; }
    
    // Form Content
    public string? HeaderText { get; set; }
    public string? SubheaderText { get; set; }
    public string SubmitButtonText { get; set; } = "Submit";
    public string? FooterText { get; set; }
    public bool ShowPoweredBy { get; set; } = true;
    
    // After Submission
    public SubmissionAction SubmissionAction { get; set; } = SubmissionAction.ShowMessage;
    public string? SuccessMessage { get; set; } = "Thank you! Your submission has been received.";
    public string? RedirectUrl { get; set; }
    public bool SendNotificationEmail { get; set; } = true;
    public string? NotificationEmail { get; set; }
    
    // Lead Settings
    public bool CreateLead { get; set; } = true;
    public int? PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }
    public int? DefaultStageId { get; set; }
    public PipelineStage? DefaultStage { get; set; }
    public string? LeadSource { get; set; } = "Website Form";
    
    // Spam Protection
    public bool EnableHoneypot { get; set; } = true;
    public bool EnableRecaptcha { get; set; } = false;
    public string? RecaptchaSiteKey { get; set; }
    public string? RecaptchaSecretKey { get; set; }
    
    // Double Opt-in
    public bool RequireDoubleOptIn { get; set; } = false;
    public string? DoubleOptInEmailTemplate { get; set; }
    
    // Status
    public FormStatus Status { get; set; } = FormStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    
    // Created By
    public string CreatedById { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    
    // Statistics (denormalized for quick access)
    public int Views { get; set; } = 0;
    public int Submissions { get; set; } = 0;
    public decimal ConversionRate { get; set; } = 0;
    
    // Unique identifier for embedding
    public string EmbedCode { get; set; } = Guid.NewGuid().ToString("N");
    
    // Navigation
    public ICollection<FormField> Fields { get; set; } = new List<FormField>();
    public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}

public class FormField
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public WebsiteForm? Form { get; set; }
    
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;
    
    // Validation
    public bool IsRequired { get; set; } = false;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; } // Regex pattern
    public string? ValidationMessage { get; set; }
    
    // Options (for select, radio, checkbox)
    public string? Options { get; set; } // JSON array of options
    public bool AllowOther { get; set; } = false;
    
    // Conditional Logic
    public bool HasConditionalLogic { get; set; } = false;
    public string? ConditionalLogic { get; set; } // JSON: { showIf: { field: "...", operator: "...", value: "..." } }
    
    // Layout
    public int Order { get; set; } = 0;
    public int Width { get; set; } = 100; // Percentage width (50, 100)
    public bool IsHidden { get; set; } = false;
    
    // Pre-fill
    public string? DefaultValue { get; set; }
    public bool PreFillFromUrl { get; set; } = false; // Allow ?fieldName=value in URL
    
    // Mapping to Lead fields
    public string? LeadFieldMapping { get; set; } // e.g., "Email", "FirstName", "Phone"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FormSubmission
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public WebsiteForm? Form { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Submitted Data (JSON)
    public string Data { get; set; } = "{}";
    
    // Associated Lead (if created)
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    // Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public string? PageUrl { get; set; }
    
    // UTM Parameters
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    
    // Status
    public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
    public bool IsSpam { get; set; } = false;
    public bool DoubleOptInConfirmed { get; set; } = false;
    public DateTime? DoubleOptInConfirmedAt { get; set; }
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class FormAnalytics
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public WebsiteForm? Form { get; set; }
    
    public DateTime Date { get; set; }
    public int Views { get; set; } = 0;
    public int Submissions { get; set; } = 0;
    public int UniqueViews { get; set; } = 0;
    public int PartialSubmissions { get; set; } = 0; // Started but not completed
    public decimal ConversionRate { get; set; } = 0;
    
    // Device breakdown
    public int DesktopViews { get; set; } = 0;
    public int MobileViews { get; set; } = 0;
    public int TabletViews { get; set; } = 0;
}

// Enums
public enum FormType
{
    ContactForm,
    LeadCapture,
    Newsletter,
    Registration,
    Survey,
    Feedback,
    Support,
    Quote,
    Booking,
    Custom
}

public enum FormStyle
{
    Standard,
    Inline,
    Floating,
    Slide,
    Modal,
    FullPage
}

public enum FormStatus
{
    Draft,
    Published,
    Archived
}

public enum SubmissionAction
{
    ShowMessage,
    RedirectToUrl,
    ShowMessageThenRedirect
}

public enum SubmissionStatus
{
    New,
    Read,
    Contacted,
    Converted,
    Spam
}

public enum FormFieldType
{
    // Basic Fields
    Text,
    Email,
    Phone,
    Number,
    Password,
    
    // Text Areas
    Textarea,
    RichText,
    
    // Selection
    Select,
    MultiSelect,
    Radio,
    Checkbox,
    CheckboxGroup,
    
    // Date/Time
    Date,
    Time,
    DateTime,
    DateRange,
    
    // Files
    FileUpload,
    ImageUpload,
    
    // Special
    Hidden,
    Consent,
    Rating,
    Scale,
    Signature,
    
    // Layout
    Heading,
    Paragraph,
    Divider,
    Spacer,
    
    // Advanced
    Country,
    State,
    Address,
    Url,
    SocialMedia
}
