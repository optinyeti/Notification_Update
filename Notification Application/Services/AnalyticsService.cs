using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public AnalyticsService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task RecordPopupViewAsync(int popupId, string? userAgent, string? ipAddress)
    {
        var popup = await _context.Popups.FindAsync(popupId);
        if (popup == null) return;

        // Update popup stats
        popup.Views++;
        
        // Create detailed analytics record
        var analytics = await GetOrCreateDailyAnalytics(popupId);
        analytics.Views++;
        
        // Update device breakdown
        if (IsPhoneBrowserString(userAgent))
            analytics.MobileViews++;
        else
            analytics.DesktopViews++;

        await _context.SaveChangesAsync();

        // Track usage for tenant
        await _tenantService.IncrementUsageAsync(popup.TenantId, "popup_views");
    }

    public async Task RecordPopupClickAsync(int popupId, string? userAgent, string? ipAddress)
    {
        var popup = await _context.Popups.FindAsync(popupId);
        if (popup == null) return;

        popup.Clicks++;
        
        var analytics = await GetOrCreateDailyAnalytics(popupId);
        analytics.Clicks++;
        
        await _context.SaveChangesAsync();
    }

    public async Task RecordPopupConversionAsync(int popupId, string? userAgent, string? ipAddress)
    {
        var popup = await _context.Popups.FindAsync(popupId);
        if (popup == null) return;

        popup.Conversions++;
        
        var analytics = await GetOrCreateDailyAnalytics(popupId);
        analytics.Conversions++;
        analytics.ConversionRate = analytics.Views > 0 ? (decimal)analytics.Conversions / analytics.Views * 100 : 0;
        
        await _context.SaveChangesAsync();
    }

    public async Task<PopupAnalytics> GetPopupAnalyticsAsync(int popupId, DateTime startDate, DateTime endDate)
    {
        var analytics = await _context.PopupAnalytics
            .Where(pa => pa.PopupId == popupId && 
                        pa.Date >= startDate.Date && 
                        pa.Date <= endDate.Date)
            .GroupBy(pa => pa.PopupId)
            .Select(g => new PopupAnalytics
            {
                PopupId = g.Key,
                Views = g.Sum(pa => pa.Views),
                Clicks = g.Sum(pa => pa.Clicks),
                Conversions = g.Sum(pa => pa.Conversions),
                MobileViews = g.Sum(pa => pa.MobileViews),
                DesktopViews = g.Sum(pa => pa.DesktopViews),
                ConversionRate = g.Sum(pa => pa.Views) > 0 ? 
                    (decimal)g.Sum(pa => pa.Conversions) / g.Sum(pa => pa.Views) * 100 : 0
            })
            .FirstOrDefaultAsync() ?? new PopupAnalytics { PopupId = popupId };

        return analytics;
    }

    public async Task<List<PopupAnalytics>> GetDailyPopupAnalyticsAsync(int popupId, DateTime startDate, DateTime endDate)
    {
        var analytics = await _context.PopupAnalytics
            .Where(pa => pa.PopupId == popupId && 
                        pa.Date >= startDate.Date && 
                        pa.Date <= endDate.Date)
            .OrderBy(pa => pa.Date)
            .ToListAsync();

        return analytics;
    }

    public async Task<IEnumerable<PopupAnalytics>> GetTenantAnalyticsAsync(int tenantId, DateTime startDate, DateTime endDate)
    {
        return await _context.PopupAnalytics
            .Include(pa => pa.Popup)
            .Where(pa => pa.Popup!.TenantId == tenantId &&
                        pa.Date >= startDate.Date &&
                        pa.Date <= endDate.Date)
            .GroupBy(pa => pa.PopupId)
            .Select(g => new PopupAnalytics
            {
                PopupId = g.Key,
                Views = g.Sum(pa => pa.Views),
                Clicks = g.Sum(pa => pa.Clicks),
                Conversions = g.Sum(pa => pa.Conversions),
                MobileViews = g.Sum(pa => pa.MobileViews),
                DesktopViews = g.Sum(pa => pa.DesktopViews),
                ConversionRate = g.Sum(pa => pa.Views) > 0 ?
                    (decimal)g.Sum(pa => pa.Conversions) / g.Sum(pa => pa.Views) * 100 : 0
            })
            .ToListAsync();
    }

    public async Task<object> GetAnalyticsSummaryAsync(int tenantId)
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var summary = await _context.PopupAnalytics
            .Include(pa => pa.Popup)
            .Where(pa => pa.Popup!.TenantId == tenantId &&
                        pa.Date >= startDate.Date &&
                        pa.Date <= endDate.Date)
            .GroupBy(pa => 1)
            .Select(g => new
            {
                TotalViews = g.Sum(pa => pa.Views),
                TotalClicks = g.Sum(pa => pa.Clicks),
                TotalConversions = g.Sum(pa => pa.Conversions),
                AverageConversionRate = g.Average(pa => pa.ConversionRate),
                MobileViews = g.Sum(pa => pa.MobileViews),
                DesktopViews = g.Sum(pa => pa.DesktopViews),
                ActivePopups = g.Select(pa => pa.PopupId).Distinct().Count()
            })
            .FirstOrDefaultAsync();

        return summary ?? new
        {
            TotalViews = 0,
            TotalClicks = 0,
            TotalConversions = 0,
            AverageConversionRate = 0.0m,
            MobileViews = 0,
            DesktopViews = 0,
            ActivePopups = 0
        };
    }

    private async Task<PopupAnalytics> GetOrCreateDailyAnalytics(int popupId)
    {
        var today = DateTime.UtcNow.Date;
        var analytics = await _context.PopupAnalytics
            .FirstOrDefaultAsync(pa => pa.PopupId == popupId && pa.Date == today);

        if (analytics == null)
        {
            analytics = new PopupAnalytics
            {
                PopupId = popupId,
                Date = today
            };
            _context.PopupAnalytics.Add(analytics);
        }

        return analytics;
    }

    private static bool IsPhoneBrowserString(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return false;
        
        return userAgent.ToLower().Contains("mobile") ||
               userAgent.ToLower().Contains("android") ||
               userAgent.ToLower().Contains("iphone") ||
               userAgent.ToLower().Contains("ipad");
    }
}