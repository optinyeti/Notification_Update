using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _context;

    public NewsletterService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Newsletter?> GetNewsletterAsync(int id, int tenantId)
    {
        return await _context.Newsletters
            .Include(n => n.CreatedBy)
            .Include(n => n.Recipients)
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId);
    }

    public async Task<IEnumerable<Newsletter>> GetNewslettersAsync(int tenantId)
    {
        return await _context.Newsletters
            .Where(n => n.TenantId == tenantId)
            .Include(n => n.CreatedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Newsletter> CreateNewsletterAsync(Newsletter newsletter)
    {
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();
        return newsletter;
    }

    public async Task<Newsletter> UpdateNewsletterAsync(Newsletter newsletter)
    {
        newsletter.UpdatedAt = DateTime.UtcNow;
        _context.Newsletters.Update(newsletter);
        await _context.SaveChangesAsync();
        return newsletter;
    }

    public async Task DeleteNewsletterAsync(int id, int tenantId)
    {
        var newsletter = await _context.Newsletters
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId);

        if (newsletter != null)
        {
            _context.Newsletters.Remove(newsletter);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SendNewsletterAsync(int id, int tenantId)
    {
        var newsletter = await GetNewsletterAsync(id, tenantId);
        if (newsletter == null || newsletter.Status != NewsletterStatus.Draft)
            return false;

        newsletter.Status = NewsletterStatus.Sending;
        newsletter.SentAt = DateTime.UtcNow;

        // In a real implementation, this would integrate with an email service
        // For now, we'll just mark as sent
        newsletter.Status = NewsletterStatus.Sent;
        newsletter.TotalSent = newsletter.Recipients.Count;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ScheduleNewsletterAsync(int id, DateTime scheduledAt, int tenantId)
    {
        var newsletter = await GetNewsletterAsync(id, tenantId);
        if (newsletter == null)
            return false;

        newsletter.Status = NewsletterStatus.Scheduled;
        newsletter.ScheduledAt = scheduledAt;

        await _context.SaveChangesAsync();
        return true;
    }
}

public class SupportService : ISupportService
{
    private readonly ApplicationDbContext _context;

    public SupportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetTicketAsync(int id, int tenantId)
    {
        return await _context.SupportTickets
            .Include(st => st.CreatedBy)
            .Include(st => st.AssignedTo)
            .Include(st => st.Messages)
                .ThenInclude(tm => tm.CreatedBy)
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == tenantId);
    }

    public async Task<IEnumerable<SupportTicket>> GetTicketsAsync(int tenantId)
    {
        return await _context.SupportTickets
            .Where(st => st.TenantId == tenantId)
            .Include(st => st.CreatedBy)
            .Include(st => st.AssignedTo)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket> CreateTicketAsync(SupportTicket ticket)
    {
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<SupportTicket> UpdateTicketAsync(SupportTicket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        _context.SupportTickets.Update(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<TicketMessage> AddMessageAsync(int ticketId, string message, string userId, bool isFromSupport = false)
    {
        var ticketMessage = new TicketMessage
        {
            TicketId = ticketId,
            Message = message,
            CreatedById = userId,
            IsFromSupport = isFromSupport
        };

        _context.TicketMessages.Add(ticketMessage);
        
        // Update ticket
        var ticket = await _context.SupportTickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            if (isFromSupport && ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
            }
        }

        await _context.SaveChangesAsync();
        return ticketMessage;
    }

    public async Task<SupportTicket> AssignTicketAsync(int ticketId, string assignedToId)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.AssignedToId = assignedToId;
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return ticket!;
    }

    public async Task<SupportTicket> CloseTicketAsync(int ticketId)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return ticket!;
    }
}

public class ApiUsageService : IApiUsageService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public ApiUsageService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task RecordApiUsageAsync(int tenantId, string endpoint, string method, int responseStatus, long responseTimeMs, string? ipAddress = null)
    {
        var apiUsage = new ApiUsage
        {
            TenantId = tenantId,
            Endpoint = endpoint,
            Method = method,
            ResponseStatus = responseStatus,
            ResponseTimeMs = responseTimeMs,
            IpAddress = ipAddress
        };

        _context.ApiUsages.Add(apiUsage);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ApiUsage>> GetApiUsageAsync(int tenantId, DateTime startDate, DateTime endDate)
    {
        return await _context.ApiUsages
            .Where(au => au.TenantId == tenantId &&
                        au.RequestDate >= startDate &&
                        au.RequestDate <= endDate)
            .OrderByDescending(au => au.RequestDate)
            .ToListAsync();
    }

    public async Task<object> GetApiUsageSummaryAsync(int tenantId)
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var summary = await _context.ApiUsages
            .Where(au => au.TenantId == tenantId &&
                        au.RequestDate >= startDate &&
                        au.RequestDate <= endDate)
            .GroupBy(au => 1)
            .Select(g => new
            {
                TotalRequests = g.Count(),
                SuccessfulRequests = g.Count(au => au.ResponseStatus >= 200 && au.ResponseStatus < 300),
                ErrorRequests = g.Count(au => au.ResponseStatus >= 400),
                AverageResponseTime = g.Average(au => au.ResponseTimeMs)
            })
            .FirstOrDefaultAsync();

        return summary ?? new
        {
            TotalRequests = 0,
            SuccessfulRequests = 0,
            ErrorRequests = 0,
            AverageResponseTime = 0.0
        };
    }

    public async Task<bool> CheckApiLimitsAsync(int tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant?.SubscriptionPlan == null)
            return false;

        return tenant.SubscriptionPlan.HasAPIAccess;
    }
}

public class IntegrationService : IIntegrationService
{
    private readonly ApplicationDbContext _context;

    public IntegrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Integration?> GetIntegrationAsync(int id, int tenantId)
    {
        return await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);
    }

    public async Task<IEnumerable<Integration>> GetIntegrationsAsync(int tenantId)
    {
        return await _context.Integrations
            .Where(i => i.TenantId == tenantId)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Integration> CreateIntegrationAsync(Integration integration)
    {
        _context.Integrations.Add(integration);
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task<Integration> UpdateIntegrationAsync(Integration integration)
    {
        _context.Integrations.Update(integration);
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task DeleteIntegrationAsync(int id, int tenantId)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);

        if (integration != null)
        {
            _context.Integrations.Remove(integration);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> TestIntegrationAsync(int id, int tenantId)
    {
        var integration = await GetIntegrationAsync(id, tenantId);
        if (integration == null)
            return false;

        // In a real implementation, this would test the actual integration
        // For now, just return true for enabled integrations
        return integration.IsEnabled;
    }

    public async Task TriggerZapierWebhookAsync(int tenantId, string eventType, object data)
    {
        var zapierIntegrations = await _context.Integrations
            .Where(i => i.TenantId == tenantId && 
                       i.Type == IntegrationType.Zapier && 
                       i.IsEnabled && 
                       !string.IsNullOrEmpty(i.WebhookUrl))
            .ToListAsync();

        foreach (var integration in zapierIntegrations)
        {
            // In a real implementation, this would make HTTP POST to webhook URL
            // For now, just update last sync time
            integration.LastSyncAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}