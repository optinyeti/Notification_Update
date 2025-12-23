using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;

namespace Notification_Application.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Popup> Popups { get; set; }
    public DbSet<PopupTemplate> PopupTemplates { get; set; }
    public DbSet<PopupAnalytics> PopupAnalytics { get; set; }
    public DbSet<EmailCapture> EmailCaptures { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogTag> BlogTags { get; set; }
    public DbSet<Newsletter> Newsletters { get; set; }
    public DbSet<NewsletterRecipient> NewsletterRecipients { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<ApiUsage> ApiUsages { get; set; }
    public DbSet<Integration> Integrations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User relationships
        builder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Tenant relationships
        builder.Entity<Tenant>(entity =>
        {
            entity.HasOne(t => t.SubscriptionPlan)
                  .WithMany(sp => sp.Tenants)
                  .HasForeignKey(t => t.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.Domain).IsUnique();
        });

        // Configure Popup relationships
        builder.Entity<Popup>(entity =>
        {
            entity.HasOne(p => p.Tenant)
                  .WithMany(t => t.Popups)
                  .HasForeignKey(p => p.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.CreatedBy)
                  .WithMany(u => u.Popups)
                  .HasForeignKey(p => p.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PopupAnalytics relationships
        builder.Entity<PopupAnalytics>(entity =>
        {
            entity.HasOne(pa => pa.Popup)
                  .WithMany(p => p.Analytics)
                  .HasForeignKey(pa => pa.PopupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pa => new { pa.PopupId, pa.Date });
        });

        // Configure EmailCapture relationships
        builder.Entity<EmailCapture>(entity =>
        {
            entity.HasOne(ec => ec.Popup)
                  .WithMany(p => p.EmailCaptures)
                  .HasForeignKey(ec => ec.PopupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ec => ec.Tenant)
                  .WithMany()
                  .HasForeignKey(ec => ec.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure BlogPost relationships
        builder.Entity<BlogPost>(entity =>
        {
            entity.HasOne(bp => bp.Tenant)
                  .WithMany(t => t.BlogPosts)
                  .HasForeignKey(bp => bp.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bp => bp.Author)
                  .WithMany()
                  .HasForeignKey(bp => bp.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(bp => bp.Slug).IsUnique();
        });

        // Configure many-to-many relationships for BlogPost
        builder.Entity<BlogPost>()
               .HasMany(bp => bp.Categories)
               .WithMany(bc => bc.BlogPosts);

        builder.Entity<BlogPost>()
               .HasMany(bp => bp.Tags)
               .WithMany(bt => bt.BlogPosts);

        // Configure Newsletter relationships
        builder.Entity<Newsletter>(entity =>
        {
            entity.HasOne(n => n.Tenant)
                  .WithMany(t => t.Newsletters)
                  .HasForeignKey(n => n.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.CreatedBy)
                  .WithMany()
                  .HasForeignKey(n => n.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure NewsletterRecipient relationships
        builder.Entity<NewsletterRecipient>(entity =>
        {
            entity.HasOne(nr => nr.Newsletter)
                  .WithMany(n => n.Recipients)
                  .HasForeignKey(nr => nr.NewsletterId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SupportTicket relationships
        builder.Entity<SupportTicket>(entity =>
        {
            entity.HasOne(st => st.Tenant)
                  .WithMany()
                  .HasForeignKey(st => st.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(st => st.CreatedBy)
                  .WithMany(u => u.SupportTickets)
                  .HasForeignKey(st => st.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(st => st.AssignedTo)
                  .WithMany()
                  .HasForeignKey(st => st.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure TicketMessage relationships
        builder.Entity<TicketMessage>(entity =>
        {
            entity.HasOne(tm => tm.Ticket)
                  .WithMany(st => st.Messages)
                  .HasForeignKey(tm => tm.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tm => tm.CreatedBy)
                  .WithMany()
                  .HasForeignKey(tm => tm.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ApiUsage relationships
        builder.Entity<ApiUsage>(entity =>
        {
            entity.HasOne(au => au.Tenant)
                  .WithMany()
                  .HasForeignKey(au => au.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(au => au.RequestDate);
        });

        // Configure Integration relationships
        builder.Entity<Integration>(entity =>
        {
            entity.HasOne(i => i.Tenant)
                  .WithMany()
                  .HasForeignKey(i => i.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default subscription plans
        builder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Free",
                Description = "Perfect for getting started",
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxPopups = 3,
                MaxPopupViews = 1000,
                MaxUsers = 1,
                HasAdvancedTargeting = false,
                HasAnalytics = false,
                HasAPIAccess = false,
                HasPrioritySupport = false,
                HasWhiteLabel = false
            },
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Professional",
                Description = "For growing businesses",
                MonthlyPrice = 29,
                YearlyPrice = 290,
                MaxPopups = 25,
                MaxPopupViews = 50000,
                MaxUsers = 5,
                HasAdvancedTargeting = true,
                HasAnalytics = true,
                HasAPIAccess = true,
                HasPrioritySupport = false,
                HasWhiteLabel = false
            },
            new SubscriptionPlan
            {
                Id = 3,
                Name = "Enterprise",
                Description = "For large organizations",
                MonthlyPrice = 99,
                YearlyPrice = 990,
                MaxPopups = -1, // Unlimited
                MaxPopupViews = -1, // Unlimited
                MaxUsers = -1, // Unlimited
                HasAdvancedTargeting = true,
                HasAnalytics = true,
                HasAPIAccess = true,
                HasPrioritySupport = true,
                HasWhiteLabel = true
            }
        );
    }
}