using Notification_Application.Models;

namespace Notification_Application.Services;

public interface ITenantService
{
    Task<Tenant?> GetTenantAsync(int tenantId);
    Task<Tenant?> GetTenantByDomainAsync(string domain);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    Task<Tenant> CreateTenantAsync(Tenant tenant);
    Task<Tenant> UpdateTenantAsync(Tenant tenant);
    Task DeleteTenantAsync(int tenantId);
    Task<bool> CheckUsageLimitsAsync(int tenantId, string resourceType);
    Task IncrementUsageAsync(int tenantId, string resourceType, int amount = 1);
    Task<bool> SyncPopupCountAsync(int tenantId);
    Task<bool> ResetPopupCountAsync(int tenantId);
}

public interface IPopupService
{
    Task<Popup?> GetPopupAsync(int popupId, int tenantId);
    Task<IEnumerable<Popup>> GetPopupsAsync(int tenantId);
    Task<IEnumerable<Popup>> GetPopupsByTypeAsync(int tenantId, PopupType type);
    Task<Popup> CreatePopupAsync(Popup popup);
    Task<Popup> UpdatePopupAsync(Popup popup);
    Task DeletePopupAsync(int popupId, int tenantId);
    Task<Popup> PublishPopupAsync(int popupId, int tenantId);
    Task<Popup> UnpublishPopupAsync(int popupId, int tenantId);
    Task<string> GeneratePopupScriptAsync(int popupId);
    Task<string> GenerateTenantScriptAsync(int tenantId);
}

public interface IAnalyticsService
{
    Task RecordPopupViewAsync(int popupId, string? userAgent, string? ipAddress);
    Task RecordPopupClickAsync(int popupId, string? userAgent, string? ipAddress);
    Task RecordPopupConversionAsync(int popupId, string? userAgent, string? ipAddress);
    Task<PopupAnalytics> GetPopupAnalyticsAsync(int popupId, DateTime startDate, DateTime endDate);
    Task<List<PopupAnalytics>> GetDailyPopupAnalyticsAsync(int popupId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<PopupAnalytics>> GetTenantAnalyticsAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<object> GetAnalyticsSummaryAsync(int tenantId);
}

public interface IBlogService
{
    Task<BlogPost?> GetBlogPostAsync(int id);
    Task<BlogPost?> GetBlogPostBySlugAsync(string slug);
    Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int tenantId, int page = 1, int pageSize = 10);
    Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
    Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost);
    Task DeleteBlogPostAsync(int id);
    Task<IEnumerable<BlogCategory>> GetCategoriesAsync(int tenantId);
    Task<IEnumerable<BlogTag>> GetTagsAsync(int tenantId);
}

public interface INewsletterService
{
    Task<Newsletter?> GetNewsletterAsync(int id, int tenantId);
    Task<IEnumerable<Newsletter>> GetNewslettersAsync(int tenantId);
    Task<Newsletter> CreateNewsletterAsync(Newsletter newsletter);
    Task<Newsletter> UpdateNewsletterAsync(Newsletter newsletter);
    Task DeleteNewsletterAsync(int id, int tenantId);
    Task<bool> SendNewsletterAsync(int id, int tenantId);
    Task<bool> ScheduleNewsletterAsync(int id, DateTime scheduledAt, int tenantId);
}

public interface ISupportService
{
    Task<SupportTicket?> GetTicketAsync(int id, int tenantId);
    Task<IEnumerable<SupportTicket>> GetTicketsAsync(int tenantId);
    Task<SupportTicket> CreateTicketAsync(SupportTicket ticket);
    Task<SupportTicket> UpdateTicketAsync(SupportTicket ticket);
    Task<TicketMessage> AddMessageAsync(int ticketId, string message, string userId, bool isFromSupport = false);
    Task<SupportTicket> AssignTicketAsync(int ticketId, string assignedToId);
    Task<SupportTicket> CloseTicketAsync(int ticketId);
}

public interface IApiUsageService
{
    Task RecordApiUsageAsync(int tenantId, string endpoint, string method, int responseStatus, long responseTimeMs, string? ipAddress = null);
    Task<IEnumerable<ApiUsage>> GetApiUsageAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<object> GetApiUsageSummaryAsync(int tenantId);
    Task<bool> CheckApiLimitsAsync(int tenantId);
}

public interface IIntegrationService
{
    Task<Integration?> GetIntegrationAsync(int id, int tenantId);
    Task<IEnumerable<Integration>> GetIntegrationsAsync(int tenantId);
    Task<Integration> CreateIntegrationAsync(Integration integration);
    Task<Integration> UpdateIntegrationAsync(Integration integration);
    Task DeleteIntegrationAsync(int id, int tenantId);
    Task<bool> TestIntegrationAsync(int id, int tenantId);
    Task TriggerZapierWebhookAsync(int tenantId, string eventType, object data);
}

public interface IPopupTemplateService
{
    Task<PopupTemplate?> GetTemplateAsync(int id);
    Task<IEnumerable<PopupTemplate>> GetAllTemplatesAsync();
    Task<IEnumerable<PopupTemplate>> GetTemplatesByTypeAsync(PopupType type);
    Task<PopupTemplate> CreateTemplateAsync(PopupTemplate template);
    Task<PopupTemplate> UpdateTemplateAsync(PopupTemplate template);
    Task DeleteTemplateAsync(int id);
}