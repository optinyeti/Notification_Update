namespace Notification_Application.Models;

/// <summary>
/// Tracks user progress through the onboarding checklist.
/// </summary>
public class OnboardingProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Onboarding steps completion status
    public bool HasAddedWebsite { get; set; } = false;
    public bool HasCreatedPopup { get; set; } = false;
    public bool HasInstalledPixel { get; set; } = false;
    public bool HasConnectedIntegration { get; set; } = false;
    public bool HasConfiguredTargeting { get; set; } = false;
    public bool HasViewedAnalytics { get; set; } = false;
    public bool HasCustomizedBranding { get; set; } = false;
    
    /// <summary>
    /// Whether the user has dismissed the onboarding checklist.
    /// </summary>
    public bool HasDismissed { get; set; } = false;
    
    /// <summary>
    /// When the onboarding was completed (all steps done).
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Calculate the percentage of onboarding completed (0-100).
    /// </summary>
    public int CompletionPercentage
    {
        get
        {
            var totalSteps = 7;
            var completedSteps = 0;
            
            if (HasAddedWebsite) completedSteps++;
            if (HasCreatedPopup) completedSteps++;
            if (HasInstalledPixel) completedSteps++;
            if (HasConnectedIntegration) completedSteps++;
            if (HasConfiguredTargeting) completedSteps++;
            if (HasViewedAnalytics) completedSteps++;
            if (HasCustomizedBranding) completedSteps++;
            
            return (int)((double)completedSteps / totalSteps * 100);
        }
    }
    
    /// <summary>
    /// Check if onboarding is complete (all steps done).
    /// </summary>
    public bool IsComplete => 
        HasAddedWebsite && 
        HasCreatedPopup && 
        HasInstalledPixel && 
        HasConnectedIntegration && 
        HasConfiguredTargeting && 
        HasViewedAnalytics && 
        HasCustomizedBranding;
}
