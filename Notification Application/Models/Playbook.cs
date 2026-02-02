using System.ComponentModel.DataAnnotations;

namespace Notification_Application.Models;

public class Playbook
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Industry { get; set; } = string.Empty; // E-commerce, SaaS, Education, etc.
    
    [Required]
    public string Tactic { get; set; } = string.Empty; // Lead Generation, Conversion, Retention, etc.
    
    public string Icon { get; set; } = "bi-bookmark-star"; // Bootstrap icon class
    
    public string IconColor { get; set; } = "blue"; // Color theme
    
    // Comma-separated template IDs
    public string TemplateIds { get; set; } = string.Empty;
    
    public string TargetingTips { get; set; } = string.Empty;
    
    public string ExpectedResults { get; set; } = string.Empty;
    
    public string BestPractices { get; set; } = string.Empty;
    
    public bool IsPopular { get; set; } = false;
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Helper method to get template IDs as list
    public List<int> GetTemplateIdList()
    {
        if (string.IsNullOrEmpty(TemplateIds))
            return new List<int>();
            
        return TemplateIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
                         .Where(id => id > 0)
                         .ToList();
    }
}
