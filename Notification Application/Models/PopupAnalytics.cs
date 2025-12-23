namespace Notification_Application.Models;

public class PopupAnalytics
{
    public int Id { get; set; }
    public int PopupId { get; set; }
    public Popup? Popup { get; set; }
    
    public DateTime Date { get; set; }
    public int Views { get; set; } = 0;
    public int Clicks { get; set; } = 0;
    public int Conversions { get; set; } = 0;
    public decimal ConversionRate { get; set; } = 0;
    
    // Device breakdown
    public int MobileViews { get; set; } = 0;
    public int DesktopViews { get; set; } = 0;
    public int TabletViews { get; set; } = 0;
    
    // Geographic data
    public string? Country { get; set; }
    public string? City { get; set; }
    
    // Referrer data
    public string? ReferrerDomain { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
}