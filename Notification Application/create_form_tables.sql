-- Create WebsiteForms table
CREATE TABLE IF NOT EXISTS WebsiteForms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TenantId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT,
    FormType INTEGER NOT NULL DEFAULT 0,
    Style INTEGER NOT NULL DEFAULT 0,
    Status INTEGER NOT NULL DEFAULT 0,
    Theme TEXT DEFAULT 'light',
    
    -- Appearance
    BackgroundColor TEXT DEFAULT '#ffffff',
    TextColor TEXT DEFAULT '#1f2937',
    ButtonColor TEXT DEFAULT '#3b82f6',
    ButtonTextColor TEXT DEFAULT '#ffffff',
    BorderColor TEXT DEFAULT '#e5e7eb',
    BorderRadius INTEGER DEFAULT 8,
    FontFamily TEXT DEFAULT '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif',
    FontSize INTEGER DEFAULT 16,
    CustomCss TEXT,
    
    -- Content
    HeaderText TEXT,
    SubheaderText TEXT,
    SubmitButtonText TEXT DEFAULT 'Submit',
    FooterText TEXT,
    ShowPoweredBy INTEGER NOT NULL DEFAULT 1,
    
    -- Submission Settings
    SubmissionAction INTEGER NOT NULL DEFAULT 0,
    SuccessMessage TEXT DEFAULT 'Thank you for your submission!',
    RedirectUrl TEXT,
    
    -- Lead Integration
    CreateLead INTEGER NOT NULL DEFAULT 1,
    LeadSource TEXT,
    PipelineId INTEGER,
    DefaultStageId INTEGER,
    AssignToUserId TEXT,
    LeadTags TEXT,
    
    -- Email Notifications
    SendEmailNotification INTEGER NOT NULL DEFAULT 0,
    NotificationEmails TEXT,
    SendConfirmationEmail INTEGER NOT NULL DEFAULT 0,
    ConfirmationEmailSubject TEXT,
    ConfirmationEmailBody TEXT,
    
    -- Spam Protection
    EnableHoneypot INTEGER NOT NULL DEFAULT 1,
    EnableRecaptcha INTEGER NOT NULL DEFAULT 0,
    RecaptchaSiteKey TEXT,
    RecaptchaSecretKey TEXT,
    
    -- Scheduling
    IsScheduled INTEGER NOT NULL DEFAULT 0,
    ScheduleStart TEXT,
    ScheduleEnd TEXT,
    
    -- Limits
    HasSubmissionLimit INTEGER NOT NULL DEFAULT 0,
    MaxSubmissions INTEGER,
    
    -- Analytics
    Views INTEGER NOT NULL DEFAULT 0,
    Submissions INTEGER NOT NULL DEFAULT 0,
    ConversionRate REAL NOT NULL DEFAULT 0,
    
    -- Embed
    EmbedCode TEXT NOT NULL,
    AllowedDomains TEXT,
    
    -- Timestamps
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    PublishedAt TEXT,
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (PipelineId) REFERENCES Pipelines(Id) ON DELETE SET NULL
);

-- Create FormFields table
CREATE TABLE IF NOT EXISTS FormFields (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FormId INTEGER NOT NULL,
    FieldType INTEGER NOT NULL DEFAULT 0,
    FieldName TEXT NOT NULL,
    Label TEXT NOT NULL,
    Placeholder TEXT,
    HelpText TEXT,
    DefaultValue TEXT,
    
    -- Validation
    IsRequired INTEGER NOT NULL DEFAULT 0,
    MinLength INTEGER,
    MaxLength INTEGER,
    ValidationPattern TEXT,
    ValidationMessage TEXT,
    
    -- Options (for select, radio, checkbox)
    Options TEXT,
    AllowOther INTEGER NOT NULL DEFAULT 0,
    
    -- Conditional Logic
    HasConditionalLogic INTEGER NOT NULL DEFAULT 0,
    ConditionalLogic TEXT,
    
    -- Lead Mapping
    LeadFieldMapping TEXT,
    
    -- Layout
    Width INTEGER DEFAULT 100,
    "Order" INTEGER NOT NULL DEFAULT 0,
    IsHidden INTEGER NOT NULL DEFAULT 0,
    
    -- URL Pre-fill
    PreFillFromUrl INTEGER NOT NULL DEFAULT 0,
    UrlParameterName TEXT,
    
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    
    FOREIGN KEY (FormId) REFERENCES WebsiteForms(Id) ON DELETE CASCADE
);

-- Create FormSubmissions table
CREATE TABLE IF NOT EXISTS FormSubmissions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FormId INTEGER NOT NULL,
    TenantId INTEGER NOT NULL,
    LeadId INTEGER,
    Data TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0,
    
    -- Tracking
    IpAddress TEXT,
    UserAgent TEXT,
    Referrer TEXT,
    PageUrl TEXT,
    
    -- UTM Parameters
    UtmSource TEXT,
    UtmMedium TEXT,
    UtmCampaign TEXT,
    UtmContent TEXT,
    UtmTerm TEXT,
    
    SubmittedAt TEXT NOT NULL,
    ProcessedAt TEXT,
    
    FOREIGN KEY (FormId) REFERENCES WebsiteForms(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (LeadId) REFERENCES Leads(Id) ON DELETE SET NULL
);

-- Create FormAnalytics table
CREATE TABLE IF NOT EXISTS FormAnalytics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FormId INTEGER NOT NULL,
    Date TEXT NOT NULL,
    Views INTEGER NOT NULL DEFAULT 0,
    Submissions INTEGER NOT NULL DEFAULT 0,
    ConversionRate REAL NOT NULL DEFAULT 0,
    
    -- Device Breakdown
    DesktopViews INTEGER NOT NULL DEFAULT 0,
    MobileViews INTEGER NOT NULL DEFAULT 0,
    TabletViews INTEGER NOT NULL DEFAULT 0,
    
    FOREIGN KEY (FormId) REFERENCES WebsiteForms(Id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS IX_WebsiteForms_TenantId ON WebsiteForms(TenantId);
CREATE INDEX IF NOT EXISTS IX_WebsiteForms_EmbedCode ON WebsiteForms(EmbedCode);
CREATE INDEX IF NOT EXISTS IX_WebsiteForms_Status ON WebsiteForms(Status);
CREATE INDEX IF NOT EXISTS IX_FormFields_FormId ON FormFields(FormId);
CREATE INDEX IF NOT EXISTS IX_FormSubmissions_FormId ON FormSubmissions(FormId);
CREATE INDEX IF NOT EXISTS IX_FormSubmissions_TenantId ON FormSubmissions(TenantId);
CREATE INDEX IF NOT EXISTS IX_FormSubmissions_LeadId ON FormSubmissions(LeadId);
CREATE INDEX IF NOT EXISTS IX_FormAnalytics_FormId_Date ON FormAnalytics(FormId, Date);

-- Show created tables
SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '%Form%';
