using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Notification_Application.Models;

public class PopupTemplate
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = "General";
    
    public PopupType Type { get; set; }
    
    // Template content
    public string Content { get; set; } = string.Empty; // JSON configuration
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? CallToAction { get; set; }
    public string? ImageUrl { get; set; }
    public string? PreviewImageUrl { get; set; }
    
    // Default settings
    public string DefaultTargetingRules { get; set; } = "{}";
    public PopupTrigger DefaultTrigger { get; set; } = PopupTrigger.OnPageLoad;
    public int DefaultDelayMs { get; set; } = 0;
    public PopupFrequency DefaultFrequency { get; set; } = PopupFrequency.OncePerSession;
    
    public bool IsActive { get; set; } = true;
    public bool IsPremium { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    
    // Type-specific configuration stored as JSON
    public string TypeSpecificOptions { get; set; } = "{}";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Helper methods for type-specific options
    public T? GetTypeSpecificOptions<T>() where T : class
    {
        if (string.IsNullOrEmpty(TypeSpecificOptions)) return null;
        return JsonSerializer.Deserialize<T>(TypeSpecificOptions);
    }
    
    public void SetTypeSpecificOptions<T>(T options) where T : class
    {
        TypeSpecificOptions = JsonSerializer.Serialize(options);
    }
}

// Type-specific options classes
public class EmailCollectorOptions
{
    public string Heading { get; set; } = "Subscribe to Our Newsletter";
    public string SubHeading { get; set; } = "Get the latest updates delivered to your inbox";
    public bool CollectName { get; set; } = true;
    public bool CollectPhone { get; set; } = false;
    public string ButtonText { get; set; } = "Subscribe";
    public string SuccessMessage { get; set; } = "Thank you for subscribing!";
    public string PrivacyText { get; set; } = "We respect your privacy";
    public bool ShowSocialIcons { get; set; } = false;
    public string BackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#333333";
    public string ButtonColor { get; set; } = "#007bff";
}

public class AdvertisingOptions
{
    public string Heading { get; set; } = "Special Offer!";
    public string SubHeading { get; set; } = "Limited time only";
    public string ImageUrl { get; set; } = string.Empty;
    public string ButtonText { get; set; } = "Shop Now";
    public string ButtonUrl { get; set; } = string.Empty;
    public bool ShowCountdown { get; set; } = false;
    public DateTime? CountdownEndDate { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#f8f9fa";
    public string ButtonColor { get; set; } = "#28a745";
}

public class LightboxOptions
{
    public string ImageUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool ShowCloseButton { get; set; } = true;
    public bool CloseOnBackdropClick { get; set; } = true;
    public string BackdropColor { get; set; } = "rgba(0,0,0,0.8)";
    public int MaxWidth { get; set; } = 800;
    public int MaxHeight { get; set; } = 600;
}

public class SpinWheelOptions
{
    public string Heading { get; set; } = "Spin to Win!";
    public string SubHeading { get; set; } = "Enter your email for a chance to win";
    public List<WheelSegment> Segments { get; set; } = new();
    public string ButtonText { get; set; } = "Spin Now";
    public bool RequireEmail { get; set; } = true;
    public string ThankYouMessage { get; set; } = "Congratulations! Check your email for your prize.";
    public bool ShowTerms { get; set; } = true;
    public string TermsText { get; set; } = "One spin per email address";
}

public class WheelSegment
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF0000";
    public int Probability { get; set; } = 100;
}

public class VideoPopupOptions
{
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoProvider { get; set; } = "youtube"; // youtube, vimeo, custom
    public bool AutoPlay { get; set; } = true;
    public bool ShowControls { get; set; } = true;
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool ShowCTA { get; set; } = false;
    public string CTAText { get; set; } = "Learn More";
    public string CTAUrl { get; set; } = string.Empty;
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 450;
}

public class CouponOptions
{
    public string Heading { get; set; } = "Exclusive Discount!";
    public string CouponCode { get; set; } = string.Empty;
    public string DiscountValue { get; set; } = "20% OFF";
    public string Description { get; set; } = "Use this code at checkout";
    public DateTime? ExpiryDate { get; set; }
    public bool RequireEmail { get; set; } = true;
    public string ButtonText { get; set; } = "Copy Code";
    public string SuccessMessage { get; set; } = "Code copied to clipboard!";
    public string BackgroundColor { get; set; } = "#fff3cd";
    public string CodeBackgroundColor { get; set; } = "#212529";
    public string CodeTextColor { get; set; } = "#ffffff";
}

public class InlineOptions
{
    public string Content { get; set; } = string.Empty;
    public string Position { get; set; } = "afterParagraph"; // afterParagraph, beforeParagraph, custom
    public int ParagraphNumber { get; set; } = 2;
    public string CustomSelector { get; set; } = string.Empty;
    public bool Sticky { get; set; } = false;
    public string BackgroundColor { get; set; } = "#e9ecef";
    public string BorderColor { get; set; } = "#dee2e6";
    public int Padding { get; set; } = 20;
}

public class MessageOptions
{
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "info"; // info, success, warning, error
    public string Position { get; set; } = "top-right"; // top-left, top-right, bottom-left, bottom-right, top-center, bottom-center
    public bool ShowIcon { get; set; } = true;
    public bool AutoDismiss { get; set; } = true;
    public int AutoDismissDelay { get; set; } = 5000;
    public bool ShowCloseButton { get; set; } = true;
    public bool ShowBorder { get; set; } = true;
    public bool EnableSound { get; set; } = false;
}
