namespace Notification_Application.Models;

/// <summary>
/// Represents a view/state in a popup campaign (e.g., initial form, success message, error, download page)
/// </summary>
public class PopupView
{
    public int Id { get; set; }
    public int PopupId { get; set; }
    public Popup? Popup { get; set; }
    
    // View Details
    public string ViewType { get; set; } = string.Empty; // "initial", "success", "error", "thank-you", "download", etc.
    public string Name { get; set; } = string.Empty; // User-friendly name
    public int Order { get; set; } = 0; // Display order for drag-and-drop
    
    // Content
    public string Content { get; set; } = string.Empty; // HTML/JSON content
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? CallToAction { get; set; }
    public string? ImageUrl { get; set; }
    
    // Conditions - when to show this view
    public string? ShowConditions { get; set; } // JSON: e.g., {"trigger": "on_submit", "formValid": true}
    
    // Navigation
    public int? NextViewId { get; set; } // Which view to show next
    public bool IsDefault { get; set; } = false; // Is this the default/initial view?
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
