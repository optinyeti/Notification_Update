using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;

    public TenantService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetTenantAsync(int tenantId)
    {
        return await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    public async Task<Tenant?> GetTenantByDomainAsync(string domain)
    {
        return await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Domain == domain);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task DeleteTenantAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckUsageLimitsAsync(int tenantId, string resourceType)
    {
        var tenant = await GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return false;

        return resourceType switch
        {
            "popups" => tenant.SubscriptionPlan.MaxPopups == -1 || tenant.PopupCount < tenant.SubscriptionPlan.MaxPopups,
            "popup_views" => tenant.SubscriptionPlan.MaxPopupViews == -1 || tenant.MonthlyPopupViews < tenant.SubscriptionPlan.MaxPopupViews,
            _ => true
        };
    }

    public async Task IncrementUsageAsync(int tenantId, string resourceType, int amount = 1)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) return;

        // Reset monthly counters if needed
        var now = DateTime.UtcNow;
        if (tenant.LastUsageReset == null || tenant.LastUsageReset.Value.Month != now.Month)
        {
            tenant.MonthlyPopupViews = 0;
            tenant.LastUsageReset = now;
        }

        switch (resourceType)
        {
            case "popups":
                tenant.PopupCount += amount;
                break;
            case "popup_views":
                tenant.MonthlyPopupViews += amount;
                break;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> SyncPopupCountAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) return false;

        var actualCount = await _context.Popups.CountAsync(p => p.TenantId == tenantId);
        tenant.PopupCount = actualCount;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPopupCountAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) return false;

        tenant.PopupCount = 0;
        await _context.SaveChangesAsync();
        return true;
    }
}