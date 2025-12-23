using Microsoft.AspNetCore.Identity;

namespace Notification_Application.Models;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Popup> Popups { get; set; } = new List<Popup>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}

public enum UserRole
{
    SuperAdmin,
    Admin,
    User
}