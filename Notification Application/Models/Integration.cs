using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Notification_Application.Models;

public class Integration
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Integration Type
    [Required]
    public IntegrationType Type { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Integration Category
    public IntegrationCategory Category { get; set; }
    
    // Connection Status
    public bool IsConnected { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Authentication - store encrypted
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    
    // OAuth details
    public string? ClientId { get; set; }
    public string? OAuthState { get; set; }
    
    // Configuration stored as JSON
    public string Settings { get; set; } = "{}";
    
    // For webhook-based integrations
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    
    // External IDs
    public string? ExternalAccountId { get; set; }
    public string? ExternalListId { get; set; } // For email marketing list
    
    // Sync settings
    public bool SyncLeads { get; set; } = true;
    public bool SyncForms { get; set; } = true;
    public bool SyncPopups { get; set; } = false;
    public bool SyncEvents { get; set; } = false;
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public string? LastError { get; set; }
    public int SyncCount { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    
    // Created by
    public string? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    
    // Helper methods
    public T? GetSettings<T>() where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Settings);
        }
        catch
        {
            return null;
        }
    }
    
    public void SetSettings<T>(T settings) where T : class
    {
        Settings = JsonSerializer.Serialize(settings);
    }
}

public enum IntegrationType
{
    // Email Marketing
    Mailchimp = 1,
    Brevo = 2, // SendinBlue
    ConvertKit = 3,
    MailerLite = 4,
    ActiveCampaign = 5,
    Drip = 6,
    AWeber = 7,
    GetResponse = 8,
    ConstantContact = 9,
    Klaviyo = 10,
    CampaignMonitor = 11,
    Listrak = 12,
    Marketo = 13,
    Pardot = 14,
    
    // CRM
    HubSpot = 50,
    Salesforce = 51,
    Pipedrive = 52,
    Zoho = 53,
    Freshsales = 54,
    Keap = 55, // Infusionsoft
    HighLevel = 56,
    Close = 57,
    
    // Automation Platforms
    Zapier = 100,
    Make = 101, // Integromat
    n8n = 102,
    Pabbly = 103,
    Integrately = 104,
    
    // Analytics
    GoogleAnalytics4 = 150,
    FacebookPixel = 151,
    GoogleTagManager = 152,
    MicrosoftClarity = 153,
    Hotjar = 154,
    Mixpanel = 155,
    Segment = 156,
    Amplitude = 157,
    
    // Communication
    Slack = 200,
    Discord = 201,
    MicrosoftTeams = 202,
    Twilio = 203,
    
    // eCommerce
    Shopify = 250,
    WooCommerce = 251,
    BigCommerce = 252,
    Magento = 253,
    
    // Custom / Webhook
    Webhook = 300,
    CustomAPI = 301
}

public enum IntegrationCategory
{
    EmailMarketing = 1,
    CRM = 2,
    Automation = 3,
    Analytics = 4,
    Communication = 5,
    eCommerce = 6,
    Custom = 7
}

// Settings classes for specific integrations
public class MailchimpSettings
{
    public string? ListId { get; set; }
    public string? DataCenter { get; set; }
    public bool DoubleOptIn { get; set; } = false;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class HubSpotSettings
{
    public string? PortalId { get; set; }
    public bool CreateContacts { get; set; } = true;
    public bool CreateDeals { get; set; } = false;
    public string? DefaultDealPipeline { get; set; }
    public string? DefaultDealStage { get; set; }
    public string? DefaultOwner { get; set; }
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public string? LeadSource { get; set; } = "Popup Manager";
}

public class ActiveCampaignSettings
{
    public string? AccountUrl { get; set; }
    public int? ListId { get; set; }
    public bool DoubleOptIn { get; set; } = false;
    public List<int> Tags { get; set; } = new();
    public int? AutomationId { get; set; }
    public Dictionary<string, string> FieldMappings { get; set; } = new();
}

public class BrevoSettings
{
    public int? ListId { get; set; }
    public bool DoubleOptIn { get; set; } = false;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public int? TemplateId { get; set; } // For double opt-in email
}

public class WebhookSettings
{
    public string? Url { get; set; }
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool RetryOnFailure { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public List<string> TriggerEvents { get; set; } = new() { "lead_created" };
    public string? SecretKey { get; set; } // For webhook signing
}

public class ZapierSettings
{
    public string? WebhookUrl { get; set; }
    public List<string> TriggerEvents { get; set; } = new() { "lead_created", "form_submitted" };
    public bool IncludeMetadata { get; set; } = true;
}

public class MakeSettings
{
    public string? WebhookUrl { get; set; }
    public List<string> TriggerEvents { get; set; } = new() { "lead_created", "form_submitted" };
    public bool IncludeMetadata { get; set; } = true;
}

public class n8nSettings
{
    public string? WebhookUrl { get; set; }
    public List<string> TriggerEvents { get; set; } = new() { "lead_created", "form_submitted" };
    public bool IncludeMetadata { get; set; } = true;
    public string? AuthHeaderName { get; set; }
    public string? AuthHeaderValue { get; set; }
}

public class GA4Settings
{
    public string? MeasurementId { get; set; }
    public string? ApiSecret { get; set; }
    public bool TrackPageViews { get; set; } = true;
    public bool TrackFormSubmissions { get; set; } = true;
    public bool TrackPopupViews { get; set; } = true;
    public bool TrackClicks { get; set; } = true;
    public bool TrackConversions { get; set; } = true;
    public Dictionary<string, string> CustomDimensions { get; set; } = new();
}

public class KlaviyoSettings
{
    public string? ListId { get; set; }
    public bool DoubleOptIn { get; set; } = false;
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    public bool TrackEvents { get; set; } = true;
}

public class SalesforceSettings
{
    public string? InstanceUrl { get; set; }
    public bool CreateLeads { get; set; } = true;
    public bool CreateContacts { get; set; } = false;
    public bool CreateOpportunities { get; set; } = false;
    public string? LeadSource { get; set; } = "Popup Manager";
    public string? DefaultOwnerId { get; set; }
    public Dictionary<string, string> FieldMappings { get; set; } = new();
}

// Integration log for tracking syncs and errors
public class IntegrationLog
{
    public int Id { get; set; }
    public int IntegrationId { get; set; }
    public Integration? Integration { get; set; }
    
    public int TenantId { get; set; }
    
    public IntegrationLogType Type { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; } // JSON with full request/response
    
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    
    public int? FormSubmissionId { get; set; }
    
    public bool IsSuccess { get; set; }
    public int? StatusCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum IntegrationLogType
{
    Sync = 1,
    LeadPush = 2,
    FormSubmission = 3,
    EventTrack = 4,
    OAuth = 5,
    Webhook = 6,
    Error = 7,
    Test = 8
}

// Integration static info (catalog)
public static class IntegrationCatalog
{
    public static List<IntegrationInfo> GetAll()
    {
        return new List<IntegrationInfo>
        {
            // Email Marketing
            new IntegrationInfo(IntegrationType.Mailchimp, IntegrationCategory.EmailMarketing,
                "Mailchimp", "Connect to Mailchimp to sync leads with your email lists",
                "https://mailchimp.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Brevo, IntegrationCategory.EmailMarketing,
                "Brevo (SendinBlue)", "Sync contacts with Brevo for email marketing automation",
                "https://www.brevo.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.ConvertKit, IntegrationCategory.EmailMarketing,
                "ConvertKit", "Add subscribers to your ConvertKit email sequences",
                "https://convertkit.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.MailerLite, IntegrationCategory.EmailMarketing,
                "MailerLite", "Grow your email list with MailerLite integration",
                "https://www.mailerlite.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.ActiveCampaign, IntegrationCategory.EmailMarketing,
                "ActiveCampaign", "Sync leads with ActiveCampaign for advanced email automation",
                "https://www.activecampaign.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.AWeber, IntegrationCategory.EmailMarketing,
                "AWeber", "Add subscribers to your AWeber email lists",
                "https://www.aweber.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.GetResponse, IntegrationCategory.EmailMarketing,
                "GetResponse", "Sync contacts with GetResponse email marketing",
                "https://www.getresponse.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.ConstantContact, IntegrationCategory.EmailMarketing,
                "Constant Contact", "Add contacts to your Constant Contact lists",
                "https://www.constantcontact.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Klaviyo, IntegrationCategory.EmailMarketing,
                "Klaviyo", "Sync leads with Klaviyo for eCommerce email marketing",
                "https://www.klaviyo.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.CampaignMonitor, IntegrationCategory.EmailMarketing,
                "Campaign Monitor", "Add subscribers to Campaign Monitor",
                "https://www.campaignmonitor.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Listrak, IntegrationCategory.EmailMarketing,
                "Listrak", "Connect with Listrak for retail email marketing",
                "https://www.listrak.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Marketo, IntegrationCategory.EmailMarketing,
                "Marketo", "Sync leads with Adobe Marketo Engage",
                "https://www.marketo.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Drip, IntegrationCategory.EmailMarketing,
                "Drip", "Add subscribers to Drip eCommerce email automation",
                "https://www.drip.com/favicon.ico", true, true, false),
            
            // CRM
            new IntegrationInfo(IntegrationType.HubSpot, IntegrationCategory.CRM,
                "HubSpot", "Sync leads and create deals in HubSpot CRM",
                "https://www.hubspot.com/favicon.ico", true, true, true),
            
            new IntegrationInfo(IntegrationType.Salesforce, IntegrationCategory.CRM,
                "Salesforce", "Push leads to Salesforce CRM",
                "https://www.salesforce.com/favicon.ico", true, true, true),
            
            new IntegrationInfo(IntegrationType.Pipedrive, IntegrationCategory.CRM,
                "Pipedrive", "Create contacts and deals in Pipedrive",
                "https://www.pipedrive.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Zoho, IntegrationCategory.CRM,
                "Zoho CRM", "Sync leads with Zoho CRM",
                "https://www.zoho.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Freshsales, IntegrationCategory.CRM,
                "Freshsales", "Push leads to Freshsales CRM",
                "https://www.freshworks.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Keap, IntegrationCategory.CRM,
                "Keap (Infusionsoft)", "Sync contacts with Keap",
                "https://keap.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.HighLevel, IntegrationCategory.CRM,
                "GoHighLevel", "Sync leads with HighLevel",
                "https://www.gohighlevel.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Close, IntegrationCategory.CRM,
                "Close CRM", "Push leads to Close.com",
                "https://close.com/favicon.ico", true, true, false),
            
            // Automation
            new IntegrationInfo(IntegrationType.Zapier, IntegrationCategory.Automation,
                "Zapier", "Connect to 6,000+ apps via Zapier webhooks",
                "https://zapier.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.Make, IntegrationCategory.Automation,
                "Make (Integromat)", "Build powerful automations with Make",
                "https://www.make.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.n8n, IntegrationCategory.Automation,
                "n8n", "Self-hosted automation with n8n webhooks",
                "https://n8n.io/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.Pabbly, IntegrationCategory.Automation,
                "Pabbly Connect", "Automate workflows with Pabbly Connect",
                "https://www.pabbly.com/favicon.ico", false, false, false),
            
            // Analytics
            new IntegrationInfo(IntegrationType.GoogleAnalytics4, IntegrationCategory.Analytics,
                "Google Analytics 4", "Track popup events in GA4 for conversion optimization",
                "https://www.google.com/favicon.ico", true, false, false),
            
            new IntegrationInfo(IntegrationType.FacebookPixel, IntegrationCategory.Analytics,
                "Meta Pixel", "Track conversions with Facebook/Meta Pixel",
                "https://www.facebook.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.GoogleTagManager, IntegrationCategory.Analytics,
                "Google Tag Manager", "Manage all tracking via GTM",
                "https://www.google.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.MicrosoftClarity, IntegrationCategory.Analytics,
                "Microsoft Clarity", "Analyze user behavior with Clarity heatmaps",
                "https://clarity.microsoft.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.Hotjar, IntegrationCategory.Analytics,
                "Hotjar", "Track popup interactions with Hotjar",
                "https://www.hotjar.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.Mixpanel, IntegrationCategory.Analytics,
                "Mixpanel", "Track product analytics with Mixpanel",
                "https://mixpanel.com/favicon.ico", true, false, false),
            
            new IntegrationInfo(IntegrationType.Segment, IntegrationCategory.Analytics,
                "Segment", "Route data to all your tools via Segment",
                "https://segment.com/favicon.ico", true, false, false),
            
            // Communication
            new IntegrationInfo(IntegrationType.Slack, IntegrationCategory.Communication,
                "Slack", "Get notified in Slack when leads convert",
                "https://slack.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Discord, IntegrationCategory.Communication,
                "Discord", "Send lead notifications to Discord",
                "https://discord.com/favicon.ico", false, false, false),
            
            new IntegrationInfo(IntegrationType.MicrosoftTeams, IntegrationCategory.Communication,
                "Microsoft Teams", "Post lead alerts to Teams channels",
                "https://www.microsoft.com/favicon.ico", true, true, false),
            
            new IntegrationInfo(IntegrationType.Twilio, IntegrationCategory.Communication,
                "Twilio", "Send SMS notifications when leads convert",
                "https://www.twilio.com/favicon.ico", true, false, false),
            
            // eCommerce
            new IntegrationInfo(IntegrationType.Shopify, IntegrationCategory.eCommerce,
                "Shopify", "Connect with your Shopify store",
                "https://www.shopify.com/favicon.ico", true, true, true),
            
            new IntegrationInfo(IntegrationType.WooCommerce, IntegrationCategory.eCommerce,
                "WooCommerce", "Sync with WooCommerce customers",
                "https://woocommerce.com/favicon.ico", true, true, false),
            
            // Custom
            new IntegrationInfo(IntegrationType.Webhook, IntegrationCategory.Custom,
                "Custom Webhook", "Send data to any URL endpoint",
                null, false, false, false),
            
            new IntegrationInfo(IntegrationType.CustomAPI, IntegrationCategory.Custom,
                "Custom API", "Connect to any REST API",
                null, true, false, false)
        };
    }
    
    public static IntegrationInfo? GetInfo(IntegrationType type)
    {
        return GetAll().FirstOrDefault(i => i.Type == type);
    }
    
    public static List<IntegrationInfo> GetByCategory(IntegrationCategory category)
    {
        return GetAll().Where(i => i.Category == category).ToList();
    }
}

public class IntegrationInfo
{
    public IntegrationType Type { get; set; }
    public IntegrationCategory Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? IconUrl { get; set; }
    public bool RequiresApiKey { get; set; }
    public bool SupportsOAuth { get; set; }
    public bool IsPremium { get; set; }
    
    public IntegrationInfo(IntegrationType type, IntegrationCategory category, 
        string name, string description, string? iconUrl, 
        bool requiresApiKey, bool supportsOAuth, bool isPremium)
    {
        Type = type;
        Category = category;
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        RequiresApiKey = requiresApiKey;
        SupportsOAuth = supportsOAuth;
        IsPremium = isPremium;
    }
}
