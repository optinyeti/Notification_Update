using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Services;

public class PopupService : IPopupService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;

    public PopupService(ApplicationDbContext context, ITenantService tenantService, IConfiguration configuration)
    {
        _context = context;
        _tenantService = tenantService;
        _configuration = configuration;
    }

    public async Task<Popup?> GetPopupAsync(int popupId, int tenantId)
    {
        return await _context.Popups
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == popupId && p.TenantId == tenantId);
    }

    public async Task<IEnumerable<Popup>> GetPopupsAsync(int tenantId)
    {
        return await _context.Popups
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.CreatedBy)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Popup>> GetPopupsByTypeAsync(int tenantId, PopupType type)
    {
        return await _context.Popups
            .Where(p => p.TenantId == tenantId && p.Type == type)
            .Include(p => p.CreatedBy)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Popup> CreatePopupAsync(Popup popup)
    {
        // Check usage limits
        var canCreate = await _tenantService.CheckUsageLimitsAsync(popup.TenantId, "popups");
        if (!canCreate)
        {
            throw new InvalidOperationException("Popup limit exceeded for current subscription plan.");
        }

        _context.Popups.Add(popup);
        await _context.SaveChangesAsync();

        // Increment usage count
        await _tenantService.IncrementUsageAsync(popup.TenantId, "popups");

        return popup;
    }

    public async Task<Popup> UpdatePopupAsync(Popup popup)
    {
        popup.UpdatedAt = DateTime.UtcNow;
        _context.Popups.Update(popup);
        await _context.SaveChangesAsync();
        return popup;
    }

    public async Task DeletePopupAsync(int popupId, int tenantId)
    {
        var popup = await _context.Popups
            .FirstOrDefaultAsync(p => p.Id == popupId && p.TenantId == tenantId);

        if (popup != null)
        {
            _context.Popups.Remove(popup);
            await _context.SaveChangesAsync();

            // Decrement usage count
            await _tenantService.IncrementUsageAsync(tenantId, "popups", -1);
        }
    }

    public async Task<Popup> PublishPopupAsync(int popupId, int tenantId)
    {
        var popup = await GetPopupAsync(popupId, tenantId);
        if (popup == null)
            throw new ArgumentException("Popup not found");

        popup.Status = PopupStatus.Published;
        popup.PublishedAt = DateTime.UtcNow;
        popup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return popup;
    }

    public async Task<Popup> UnpublishPopupAsync(int popupId, int tenantId)
    {
        var popup = await GetPopupAsync(popupId, tenantId);
        if (popup == null)
            throw new ArgumentException("Popup not found");

        popup.Status = PopupStatus.Paused;
        popup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return popup;
    }

    public async Task<string> GeneratePopupScriptAsync(int popupId)
    {
        var popup = await _context.Popups.FindAsync(popupId);
        if (popup == null || popup.Status != PopupStatus.Published)
            return "console.log('Popup not found or not published');";

        // Resolve host: tenant override → ProductionUrl → BaseUrl → localhost
        var tenant = await _context.Tenants.FindAsync(popup.TenantId);
        var host = tenant?.PublicHostUrl
            ?? _configuration["AppSettings:ProductionUrl"]
            ?? _configuration["AppSettings:BaseUrl"]
            ?? "http://localhost:5117";

        // Generate JavaScript embed code
        var script = $@"
(function() {{
    // Fetch popup data from API
    fetch('{host}/Popup/GetPopupData/{popup.Id}')
        .then(response => response.json())
        .then(popupData => {{
            // Load popup engine
            var script = document.createElement('script');
            script.src = '{host}/js/popup-engine.js';
            script.onload = function() {{
                if (window.PopupManager) {{
                    window.PopupManager.init(popupData);
                }}
            }};
            document.head.appendChild(script);
        }})
        .catch(error => console.error('Error loading popup:', error));
}})();";

        return script;
    }

    public async Task<string> GenerateTenantScriptAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return "console.log('Tenant not found');";

        // Resolve host: tenant override → ProductionUrl → BaseUrl → localhost
        var host = tenant.PublicHostUrl
            ?? _configuration["AppSettings:ProductionUrl"]
            ?? _configuration["AppSettings:BaseUrl"]
            ?? "http://localhost:5117";

        // Generate JavaScript embed code for tenant (loads all published popups)
        // This is the GLOBAL PIXEL - loads once and manages all campaigns
        var script = $@"
(function() {{
    // Check if PopupManager is already loaded
    if (window.PopupManagerLoaded) {{
        return;
    }}
    window.PopupManagerLoaded = true;

    // Fetch all published popups for this tenant
    fetch('{host}/Popup/GetTenantPopups/{tenantId}')
        .then(response => response.json())
        .then(popupsData => {{
            // Load popup engine with API URL
            var script = document.createElement('script');
            script.src = '{host}/js/popup-engine.js';
            script.setAttribute('data-api-url', '{host}');
            script.onload = function() {{
                if (window.PopupManager) {{
                    // Initialize with tenant ID for global tracking
                    window.PopupManager.initMultiple(popupsData, {tenantId});
                }}
            }};
            document.head.appendChild(script);
        }})
        .catch(error => console.error('Error loading popups:', error));
}})();";

        return script;
    }
}