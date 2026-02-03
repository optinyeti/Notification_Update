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
    public DbSet<IntegrationLog> IntegrationLogs { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<PopupView> PopupViews { get; set; }
    public DbSet<MediaLibraryImage> MediaLibraryImages { get; set; }
    public DbSet<Playbook> Playbooks { get; set; }
    
    // Website & Landing Page Management
    public DbSet<AllowedWebsite> AllowedWebsites { get; set; }
    public DbSet<LandingPage> LandingPages { get; set; }
    public DbSet<WebsitePage> WebsitePages { get; set; }
    public DbSet<OnboardingProgress> OnboardingProgress { get; set; }
    
    // CRM DbSets
    public DbSet<LeadActivity> LeadActivities { get; set; }
    public DbSet<Pipeline> Pipelines { get; set; }
    public DbSet<PipelineStage> PipelineStages { get; set; }
    public DbSet<LeadStageHistory> LeadStageHistories { get; set; }
    public DbSet<LeadScore> LeadScores { get; set; }
    public DbSet<LeadTag> LeadTags { get; set; }
    public DbSet<LeadCustomField> LeadCustomFields { get; set; }
    public DbSet<CrmTask> CrmTasks { get; set; }
    public DbSet<Deal> Deals { get; set; }
    
    // Website Forms DbSets
    public DbSet<WebsiteForm> WebsiteForms { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<FormAnalytics> FormAnalytics { get; set; }
    
    // Phone Tracking DbSets
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<PhoneCall> PhoneCalls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

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

        // Configure Deal relationships
        builder.Entity<Deal>(entity =>
        {
            // Deal belongs to a Lead (one-to-many: Lead has many Deals)
            entity.HasOne(d => d.Lead)
                  .WithMany(l => l.Deals)
                  .HasForeignKey(d => d.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Tenant)
                  .WithMany()
                  .HasForeignKey(d => d.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Owner)
                  .WithMany()
                  .HasForeignKey(d => d.OwnerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Lead's AssignedDeal (separate optional relationship)
        builder.Entity<Lead>(entity =>
        {
            entity.HasOne(l => l.AssignedDeal)
                  .WithMany()
                  .HasForeignKey(l => l.AssignedDealId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed default subscription plans
        builder.Entity<SubscriptionPlan>().HasData(
            // FREE PLAN
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Free",
                Description = "Perfect for testing and small projects",
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxPopups = 1,
                MaxPopupViews = 1000,
                MaxForms = 1,
                MaxLandingPages = 0,
                MaxWebsites = 1,
                MaxUsers = 1,
                HasAdvancedTargeting = false,
                HasAnalytics = true,
                HasAPIAccess = false,
                HasPrioritySupport = false,
                HasWhiteLabel = false,
                HasLandingPageBuilder = false,
                CanRemoveBranding = false,
                HasRoleBasedAccess = false,
                MaxAIRequests = 0,
                MaxContacts = 100,
                MaxPipelines = 1,
                MaxFormSubmissions = 50,
                CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            // STARTER PLAN - $29/mo
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Starter",
                Description = "Everything you need to start growing",
                MonthlyPrice = 29,
                YearlyPrice = 288, // $24/mo billed annually
                MaxPopups = 3,
                MaxPopupViews = 10000,
                MaxForms = 5,
                MaxLandingPages = 3,
                MaxWebsites = 1,
                MaxUsers = 2,
                HasAdvancedTargeting = true,
                HasAnalytics = true,
                HasAPIAccess = false,
                HasPrioritySupport = false,
                HasWhiteLabel = false,
                HasLandingPageBuilder = true,
                CanRemoveBranding = false,
                HasRoleBasedAccess = false,
                MaxAIRequests = 0,
                MaxContacts = 1000,
                MaxPipelines = 2,
                MaxFormSubmissions = 500,
                CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                StripeProductId = "prod_starter_2026",
                StripePriceIdMonthly = "price_starter_monthly_29",
                StripePriceIdYearly = "price_starter_yearly_288"
            },
            // PROFESSIONAL PLAN - $79/mo (MOST POPULAR)
            new SubscriptionPlan
            {
                Id = 3,
                Name = "Professional",
                Description = "AI-powered growth for serious businesses",
                MonthlyPrice = 79,
                YearlyPrice = 804, // $67/mo billed annually
                MaxPopups = 15,
                MaxPopupViews = 100000,
                MaxForms = 25,
                MaxLandingPages = 25,
                MaxWebsites = 5,
                MaxUsers = 10,
                HasAdvancedTargeting = true,
                HasAnalytics = true,
                HasAPIAccess = true,
                HasPrioritySupport = false,
                HasWhiteLabel = true,
                HasLandingPageBuilder = true,
                CanRemoveBranding = true,
                HasRoleBasedAccess = true,
                MaxAIRequests = 500,
                MaxContacts = 25000,
                MaxPipelines = 10,
                MaxFormSubmissions = 5000,
                CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                StripeProductId = "prod_professional_2026",
                StripePriceIdMonthly = "price_professional_monthly_79",
                StripePriceIdYearly = "price_professional_yearly_804"
            },
            // ENTERPRISE PLAN - $199/mo
            new SubscriptionPlan
            {
                Id = 4,
                Name = "Enterprise",
                Description = "Unlimited power for large organizations",
                MonthlyPrice = 199,
                YearlyPrice = 2028, // $169/mo billed annually
                MaxPopups = -1, // Unlimited
                MaxPopupViews = -1, // Unlimited
                MaxForms = -1,
                MaxLandingPages = -1,
                MaxWebsites = -1,
                MaxUsers = -1, // Unlimited
                HasAdvancedTargeting = true,
                HasAnalytics = true,
                HasAPIAccess = true,
                HasPrioritySupport = true,
                HasWhiteLabel = true,
                HasLandingPageBuilder = true,
                CanRemoveBranding = true,
                HasRoleBasedAccess = true,
                MaxAIRequests = -1, // Unlimited AI
                MaxContacts = -1,
                MaxPipelines = -1,
                MaxFormSubmissions = -1,
                CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                StripeProductId = "prod_enterprise_2026",
                StripePriceIdMonthly = "price_enterprise_monthly_199",
                StripePriceIdYearly = "price_enterprise_yearly_2028"
            }
        );
    }
}