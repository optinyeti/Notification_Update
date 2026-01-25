using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification_Application.Services;

public interface IBillingService
{
    // Usage tracking
    Task<UsageStats> GetUsageStatsAsync(int tenantId);
    Task<Dictionary<string, int>> GetFeatureUsageAsync(int tenantId);
    Task<bool> CanCreatePopupAsync(int tenantId);
    Task<bool> CanUseAdvancedTargetingAsync(int tenantId);
    Task<bool> CanUseAnalyticsAsync(int tenantId);
    Task<bool> CanUseApiAsync(int tenantId);
    Task<int> GetRemainingPopupsAsync(int tenantId);
    
    // Billing information
    Task<BillingInfo> GetBillingInfoAsync(int tenantId);
    Task<List<BillingEvent>> GetBillingHistoryAsync(int tenantId, int months = 12);
    Task<decimal> GetEstimatedMonthlyChargeAsync(int tenantId);
    
    // Usage reset (for monthly billing)
    Task<bool> ResetMonthlyUsageAsync(int tenantId);
}

public class BillingService : IBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public BillingService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<UsageStats> GetUsageStatsAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return new UsageStats();

        var plan = tenant.SubscriptionPlan;
        
        // Get actual popup count from database
        var actualPopupCount = await _context.Popups.CountAsync(p => p.TenantId == tenantId);
        
        // Reset monthly views if needed
        var now = DateTime.UtcNow;
        if (tenant.LastUsageReset == null || tenant.LastUsageReset.Value.Month != now.Month)
        {
            tenant.MonthlyPopupViews = 0;
            tenant.LastUsageReset = now;
            await _context.SaveChangesAsync();
        }

        var usageStats = new UsageStats
        {
            TenantId = tenantId,
            PlanName = plan.Name,
            PlanId = plan.Id,
            
            // Popups
            PopupsCreated = actualPopupCount,
            MaxPopups = plan.MaxPopups == -1 ? null : plan.MaxPopups,
            PopupsRemaining = plan.MaxPopups == -1 ? null : Math.Max(0, plan.MaxPopups - actualPopupCount),
            PopupUsagePercentage = plan.MaxPopups == -1 ? 0 : (actualPopupCount * 100 / Math.Max(1, plan.MaxPopups)),
            
            // Monthly Views
            MonthlyViews = tenant.MonthlyPopupViews,
            MaxMonthlyViews = plan.MaxPopupViews == -1 ? null : plan.MaxPopupViews,
            ViewsRemaining = plan.MaxPopupViews == -1 ? null : Math.Max(0, plan.MaxPopupViews - tenant.MonthlyPopupViews),
            ViewsUsagePercentage = plan.MaxPopupViews == -1 ? 0 : (tenant.MonthlyPopupViews * 100 / Math.Max(1, plan.MaxPopupViews)),
            
            // Team members
            TeamMembers = await _context.Users.CountAsync(u => u.TenantId == tenantId),
            MaxTeamMembers = plan.MaxUsers == -1 ? null : plan.MaxUsers,
            
            // Feature access
            HasAdvancedTargeting = plan.HasAdvancedTargeting,
            HasAnalytics = plan.HasAnalytics,
            HasAPIAccess = plan.HasAPIAccess,
            HasPrioritySupport = plan.HasPrioritySupport,
            HasWhiteLabel = plan.HasWhiteLabel,
            
            // Subscription info
            SubscriptionStatus = tenant.SubscriptionStatus ?? "none",
            SubscriptionStartDate = tenant.SubscriptionStartDate,
            SubscriptionEndDate = tenant.SubscriptionEndDate
        };

        return usageStats;
    }

    public async Task<Dictionary<string, int>> GetFeatureUsageAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return new Dictionary<string, int>();

        var stats = await GetUsageStatsAsync(tenantId);
        
        return new Dictionary<string, int>
        {
            { "popups", stats.PopupsCreated },
            { "monthly_views", stats.MonthlyViews },
            { "team_members", stats.TeamMembers },
            { "popups_remaining", stats.PopupsRemaining ?? 0 },
            { "views_remaining", stats.ViewsRemaining ?? 0 }
        };
    }

    public async Task<bool> CanCreatePopupAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return false;

        var actualPopupCount = await _context.Popups.CountAsync(p => p.TenantId == tenantId);
        
        // -1 means unlimited
        if (tenant.SubscriptionPlan.MaxPopups == -1)
            return true;

        return actualPopupCount < tenant.SubscriptionPlan.MaxPopups;
    }

    public async Task<bool> CanUseAdvancedTargetingAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        return tenant?.SubscriptionPlan?.HasAdvancedTargeting ?? false;
    }

    public async Task<bool> CanUseAnalyticsAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        return tenant?.SubscriptionPlan?.HasAnalytics ?? false;
    }

    public async Task<bool> CanUseApiAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        return tenant?.SubscriptionPlan?.HasAPIAccess ?? false;
    }

    public async Task<int> GetRemainingPopupsAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return 0;

        if (tenant.SubscriptionPlan.MaxPopups == -1)
            return int.MaxValue; // Unlimited

        var actualCount = await _context.Popups.CountAsync(p => p.TenantId == tenantId);
        return Math.Max(0, tenant.SubscriptionPlan.MaxPopups - actualCount);
    }

    public async Task<BillingInfo> GetBillingInfoAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant == null)
            return new BillingInfo();

        var usage = await GetUsageStatsAsync(tenantId);
        var plan = tenant.SubscriptionPlan;

        return new BillingInfo
        {
            TenantId = tenantId,
            TenantName = tenant.Name,
            PlanId = plan?.Id ?? 0,
            PlanName = plan?.Name ?? "No Plan",
            MonthlyPrice = plan?.MonthlyPrice ?? 0,
            YearlyPrice = plan?.YearlyPrice ?? 0,
            StripeCustomerId = tenant.StripeCustomerId,
            StripeSubscriptionId = tenant.StripeSubscriptionId,
            SubscriptionStatus = tenant.SubscriptionStatus ?? "inactive",
            SubscriptionStartDate = tenant.SubscriptionStartDate,
            SubscriptionEndDate = tenant.SubscriptionEndDate,
            CurrentUsage = usage
        };
    }

    public async Task<List<BillingEvent>> GetBillingHistoryAsync(int tenantId, int months = 12)
    {
        // This would fetch from a billing events table if you implement transaction history
        // For now, return empty list - can be expanded with proper event logging
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-months);

        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionStartDate != null && tenant.SubscriptionStartDate >= sixMonthsAgo)
        {
            return new List<BillingEvent>
            {
                new BillingEvent
                {
                    Date = tenant.SubscriptionStartDate.Value,
                    Type = "subscription_created",
                    Description = $"Subscription to {tenant.SubscriptionPlan?.Name} started",
                    Amount = tenant.SubscriptionPlan?.MonthlyPrice ?? 0
                }
            };
        }

        return new List<BillingEvent>();
    }

    public async Task<decimal> GetEstimatedMonthlyChargeAsync(int tenantId)
    {
        var billing = await GetBillingInfoAsync(tenantId);
        return billing.MonthlyPrice;
    }

    public async Task<bool> ResetMonthlyUsageAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return false;

        tenant.MonthlyPopupViews = 0;
        tenant.LastUsageReset = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}

// Models for billing service
public class UsageStats
{
    public int TenantId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int PlanId { get; set; }
    
    // Popups
    public int PopupsCreated { get; set; }
    public int? MaxPopups { get; set; }
    public int? PopupsRemaining { get; set; }
    public int PopupUsagePercentage { get; set; }
    
    // Monthly views
    public int MonthlyViews { get; set; }
    public int? MaxMonthlyViews { get; set; }
    public int? ViewsRemaining { get; set; }
    public int ViewsUsagePercentage { get; set; }
    
    // Team
    public int TeamMembers { get; set; }
    public int? MaxTeamMembers { get; set; }
    
    // Features
    public bool HasAdvancedTargeting { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasWhiteLabel { get; set; }
    
    // Subscription
    public string SubscriptionStatus { get; set; } = "none";
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

public class BillingInfo
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string SubscriptionStatus { get; set; } = "inactive";
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public UsageStats? CurrentUsage { get; set; }
}

public class BillingEvent
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // subscription_created, payment, upgrade, downgrade, etc.
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
