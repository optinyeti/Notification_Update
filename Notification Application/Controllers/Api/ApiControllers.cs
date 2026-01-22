using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Notification_Application.Services;

namespace Notification_Application.Controllers.Api;

[ApiController]
[Route("api/popup")]
public class PopupApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAnalyticsService _analyticsService;

    public PopupApiController(ApplicationDbContext context, IAnalyticsService analyticsService)
    {
        _context = context;
        _analyticsService = analyticsService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPopup(int id)
    {
        var popup = await _context.Popups
            .Where(p => p.Id == id && p.Status == PopupStatus.Published)
            .Select(p => new
            {
                p.Id,
                p.Type,
                p.Content,
                p.Title,
                p.Subtitle,
                p.CallToAction,
                p.Trigger,
                p.DelayMs,
                p.Frequency,
                p.TargetingRules,
                p.ShowOnMobile,
                p.ShowOnDesktop
            })
            .FirstOrDefaultAsync();

        if (popup == null)
            return NotFound();

        return Ok(popup);
    }

    [HttpPost("{id}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordView(int id)
    {
        try
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _analyticsService.RecordPopupViewAsync(id, userAgent, ipAddress);
            return Ok(new { success = true });
        }
        catch
        {
            return Ok(new { success = false });
        }
    }

    [HttpPost("{id}/click")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordClick(int id)
    {
        try
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _analyticsService.RecordPopupClickAsync(id, userAgent, ipAddress);
            return Ok(new { success = true });
        }
        catch
        {
            return Ok(new { success = false });
        }
    }

    [HttpPost("{id}/conversion")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordConversion(int id)
    {
        try
        {
            Console.WriteLine($"=== CONVERSION ENDPOINT HIT: Popup ID {id} ===");
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _analyticsService.RecordPopupConversionAsync(id, userAgent, ipAddress);
            
            Console.WriteLine($"Converting for popup {id}, calling SendToIntegrations...");
            // Send to integrations (Zapier, Webhooks)
            await SendToIntegrations(id, null, userAgent, ipAddress);
            Console.WriteLine($"SendToIntegrations completed for popup {id}");
            
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in RecordConversion: {ex.Message}");
            return Ok(new { success = false });
        }
    }

    private async Task SendToIntegrations(int popupId, Dictionary<string, object>? conversionData, string? userAgent, string? ipAddress)
    {
        try
        {
            Console.WriteLine($"=== SendToIntegrations START for popup {popupId} ===");
            var popup = await _context.Popups.FindAsync(popupId);
            
            if (popup == null)
            {
                Console.WriteLine($"ERROR: Popup {popupId} not found in database");
                return;
            }
            
            Console.WriteLine($"Popup found: {popup.Name}, TargetingRules length: {popup.TargetingRules?.Length ?? 0}");
            
            if (string.IsNullOrEmpty(popup.TargetingRules))
            {
                Console.WriteLine($"ERROR: TargetingRules is empty for popup {popupId}");
                return;
            }

            Console.WriteLine($"TargetingRules content: {popup.TargetingRules}");

            // Parse targeting rules to get integration configs
            var targetingRules = System.Text.Json.JsonDocument.Parse(popup.TargetingRules);
            Console.WriteLine($"TargetingRules parsed successfully");
            
            // Prepare payload
            var payload = new
            {
                popupId = popupId,
                popupName = popup.Name,
                timestamp = DateTime.UtcNow,
                userAgent = userAgent,
                ipAddress = ipAddress,
                conversionData = conversionData ?? new Dictionary<string, object>()
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            Console.WriteLine($"Payload prepared: {jsonPayload}");
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Check for Zapier integration
            Console.WriteLine($"Checking for zapier property...");
            if (targetingRules.RootElement.TryGetProperty("zapier", out var zapierConfig))
            {
                Console.WriteLine($"Found zapier property");
                Console.WriteLine($"Zapier config: {zapierConfig.ToString()}");
                
                if (zapierConfig.TryGetProperty("enabled", out var zapierEnabled))
                {
                    Console.WriteLine($"Zapier enabled value: {zapierEnabled.GetBoolean()}");
                    
                    if (zapierEnabled.GetBoolean())
                    {
                        Console.WriteLine($"Zapier is ENABLED");
                        
                        if (zapierConfig.TryGetProperty("webhookUrl", out var webhookUrl))
                        {
                            var url = webhookUrl.GetString();
                            Console.WriteLine($"Zapier webhook URL found: {url}");
                            
                            try
                            {
                                using var httpClient = new HttpClient();
                                httpClient.Timeout = TimeSpan.FromSeconds(10);
                                Console.WriteLine($"Sending POST request to Zapier...");
                                var response = await httpClient.PostAsync(url, content);
                                Console.WriteLine($"✓ Zapier webhook SENT to {url}: Status={response.StatusCode}");
                                var responseBody = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"Zapier response: {responseBody}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"✗ FAILED to send Zapier webhook: {ex.Message}");
                                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"ERROR: webhookUrl property not found in zapier config");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Zapier is DISABLED");
                    }
                }
                else
                {
                    Console.WriteLine($"ERROR: enabled property not found in zapier config");
                }
            }
            else
            {
                Console.WriteLine($"No zapier property found in targeting rules");
            }

            // Check for custom webhook integration
            if (targetingRules.RootElement.TryGetProperty("webhook", out var webhookConfig))
            {
                if (webhookConfig.TryGetProperty("enabled", out var webhookEnabled) && webhookEnabled.GetBoolean())
                {
                    if (webhookConfig.TryGetProperty("url", out var webhookUrlProp))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            
                            // Get method (default POST)
                            var method = webhookConfig.TryGetProperty("method", out var methodProp) 
                                ? methodProp.GetString() ?? "POST" 
                                : "POST";

                            // Add custom headers if provided
                            if (webhookConfig.TryGetProperty("headers", out var headersProp))
                            {
                                var headersText = headersProp.GetString();
                                if (!string.IsNullOrEmpty(headersText))
                                {
                                    foreach (var line in headersText.Split('\n'))
                                    {
                                        var parts = line.Split(':', 2);
                                        if (parts.Length == 2)
                                        {
                                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                                                parts[0].Trim(), 
                                                parts[1].Trim()
                                            );
                                        }
                                    }
                                }
                            }

                            var request = new HttpRequestMessage(
                                new HttpMethod(method), 
                                webhookUrlProp.GetString()
                            )
                            {
                                Content = content
                            };

                            var response = await httpClient.SendAsync(request);
                            Console.WriteLine($"Webhook sent to {webhookUrlProp.GetString()}: {response.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send webhook: {ex.Message}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"=== SendToIntegrations END for popup {popupId} ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ CRITICAL ERROR in SendToIntegrations: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

[ApiController]
[Route("api/popup-analytics")]
public class PopupAnalyticsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPopupService _popupService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IApiUsageService _apiUsageService;

    public PopupAnalyticsApiController(
        ApplicationDbContext context,
        IPopupService popupService,
        IAnalyticsService analyticsService,
        IApiUsageService apiUsageService)
    {
        _context = context;
        _popupService = popupService;
        _analyticsService = analyticsService;
        _apiUsageService = apiUsageService;
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> RecordView(int id, [FromBody] PopupEventRequest request)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _analyticsService.RecordPopupViewAsync(id, request.UserAgent, request.IpAddress);
            
            await RecordApiUsage(request.TenantId, "view", 200, startTime);
            return Ok(new { success = true });
        }
        catch (Exception)
        {
            await RecordApiUsage(request.TenantId, "view", 500, startTime);
            return StatusCode(500);
        }
    }

    [HttpPost("{id}/click")]
    public async Task<IActionResult> RecordClick(int id, [FromBody] PopupEventRequest request)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _analyticsService.RecordPopupClickAsync(id, request.UserAgent, request.IpAddress);
            
            await RecordApiUsage(request.TenantId, "click", 200, startTime);
            return Ok(new { success = true });
        }
        catch (Exception)
        {
            await RecordApiUsage(request.TenantId, "click", 500, startTime);
            return StatusCode(500);
        }
    }

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> RecordConversion(int id, [FromBody] PopupConversionRequest request)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _analyticsService.RecordPopupConversionAsync(id, request.UserAgent, request.IpAddress);
            
            // Store email capture if provided
            if (!string.IsNullOrEmpty(request.Email))
            {
                // This would typically be handled by a separate service
                // For now, we'll just record the conversion
            }
            
            // Send to integrations
            var conversionData = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.Email))
                conversionData["email"] = request.Email;
            if (!string.IsNullOrEmpty(request.FirstName))
                conversionData["firstName"] = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                conversionData["lastName"] = request.LastName;
            if (!string.IsNullOrEmpty(request.Phone))
                conversionData["phone"] = request.Phone;
                
            await SendToIntegrations(id, conversionData, request.UserAgent, request.IpAddress);
            
            await RecordApiUsage(request.TenantId, "convert", 200, startTime);
            return Ok(new { success = true });
        }
        catch (Exception)
        {
            await RecordApiUsage(request.TenantId, "convert", 500, startTime);
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPopup(int id, [FromQuery] int tenantId)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var popup = await _popupService.GetPopupAsync(id, tenantId);
            if (popup == null)
            {
                await RecordApiUsage(tenantId, "get", 404, startTime);
                return NotFound();
            }

            await RecordApiUsage(tenantId, "get", 200, startTime);
            return Ok(new PopupApiResponse
            {
                Id = popup.Id,
                Type = popup.Type.ToString(),
                Content = popup.Content,
                TargetingRules = popup.TargetingRules,
                Trigger = popup.Trigger.ToString(),
                DelayMs = popup.DelayMs,
                Frequency = popup.Frequency.ToString(),
                ShowOnMobile = popup.ShowOnMobile,
                ShowOnDesktop = popup.ShowOnDesktop,
                Status = popup.Status.ToString()
            });
        }
        catch (Exception)
        {
            await RecordApiUsage(tenantId, "get", 500, startTime);
            return StatusCode(500);
        }
    }

    private async Task RecordApiUsage(int tenantId, string action, int statusCode, DateTime startTime)
    {
        var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        var endpoint = $"/api/popup-analytics/{action}";
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        await _apiUsageService.RecordApiUsageAsync(
            tenantId, 
            endpoint, 
            Request.Method, 
            statusCode, 
            responseTime, 
            ipAddress);
    }

    private async Task SendToIntegrations(int popupId, Dictionary<string, object>? conversionData, string? userAgent, string? ipAddress)
    {
        try
        {
            var popup = await _context.Popups.FindAsync(popupId);
            if (popup == null || string.IsNullOrEmpty(popup.TargetingRules))
                return;

            // Parse targeting rules to get integration configs
            var targetingRules = System.Text.Json.JsonDocument.Parse(popup.TargetingRules);
            
            // Prepare payload
            var payload = new
            {
                popupId = popupId,
                popupName = popup.Name,
                timestamp = DateTime.UtcNow,
                userAgent = userAgent,
                ipAddress = ipAddress,
                conversionData = conversionData ?? new Dictionary<string, object>()
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Check for Zapier integration
            if (targetingRules.RootElement.TryGetProperty("zapier", out var zapierConfig))
            {
                if (zapierConfig.TryGetProperty("enabled", out var zapierEnabled) && zapierEnabled.GetBoolean())
                {
                    if (zapierConfig.TryGetProperty("webhookUrl", out var webhookUrl))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            var response = await httpClient.PostAsync(webhookUrl.GetString(), content);
                            Console.WriteLine($"Zapier webhook sent to {webhookUrl.GetString()}: {response.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send Zapier webhook: {ex.Message}");
                        }
                    }
                }
            }

            // Check for custom webhook integration
            if (targetingRules.RootElement.TryGetProperty("webhook", out var webhookConfig))
            {
                if (webhookConfig.TryGetProperty("enabled", out var webhookEnabled) && webhookEnabled.GetBoolean())
                {
                    if (webhookConfig.TryGetProperty("url", out var webhookUrlProp))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            
                            // Get method (default POST)
                            var method = webhookConfig.TryGetProperty("method", out var methodProp) 
                                ? methodProp.GetString() ?? "POST" 
                                : "POST";

                            // Add custom headers if provided
                            if (webhookConfig.TryGetProperty("headers", out var headersProp))
                            {
                                var headersText = headersProp.GetString();
                                if (!string.IsNullOrEmpty(headersText))
                                {
                                    foreach (var line in headersText.Split('\n'))
                                    {
                                        var parts = line.Split(':', 2);
                                        if (parts.Length == 2)
                                        {
                                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                                                parts[0].Trim(), 
                                                parts[1].Trim()
                                            );
                                        }
                                    }
                                }
                            }

                            var request = new HttpRequestMessage(
                                new HttpMethod(method), 
                                webhookUrlProp.GetString()
                            )
                            {
                                Content = content
                            };

                            var response = await httpClient.SendAsync(request);
                            Console.WriteLine($"Webhook sent to {webhookUrlProp.GetString()}: {response.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send webhook: {ex.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendToIntegrations: {ex.Message}");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly UserManager<User> _userManager;

    public AnalyticsController(IAnalyticsService analyticsService, UserManager<User> userManager)
    {
        _analyticsService = analyticsService;
        _userManager = userManager;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var summary = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);
        return Ok(summary);
    }

    [HttpGet("popup/{id}")]
    public async Task<IActionResult> GetPopupAnalytics(int id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var analytics = await _analyticsService.GetPopupAnalyticsAsync(id, start, end);
        return Ok(analytics);
    }

    [HttpGet("tenant")]
    public async Task<IActionResult> GetTenantAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var analytics = await _analyticsService.GetTenantAnalyticsAsync(user.TenantId, start, end);
        return Ok(analytics);
    }
}

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IIntegrationService _integrationService;

    public WebhookController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpPost("zapier/{tenantId}")]
    public async Task<IActionResult> ZapierWebhook(int tenantId, [FromBody] object data)
    {
        try
        {
            await _integrationService.TriggerZapierWebhookAsync(tenantId, "webhook_received", data);
            return Ok(new { success = true });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}

[ApiController]
[Route("api/tracking")]
[AllowAnonymous]
public class TrackingController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ApplicationDbContext _context;

    public TrackingController(IAnalyticsService analyticsService, ApplicationDbContext context)
    {
        _analyticsService = analyticsService;
        _context = context;
    }

    [HttpPost("event")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackingEventRequest request)
    {
        try
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            switch (request.EventType?.ToLower())
            {
                case "impression":
                    await _analyticsService.RecordPopupViewAsync(request.CampaignId, userAgent, ipAddress);
                    break;
                case "click":
                    await _analyticsService.RecordPopupClickAsync(request.CampaignId, userAgent, ipAddress);
                    break;
                case "conversion":
                    await _analyticsService.RecordPopupConversionAsync(request.CampaignId, userAgent, ipAddress);
                    
                    // Store conversion data if provided
                    if (request.ConversionData != null)
                    {
                        // Store email capture or other conversion data
                        // This can be extended based on requirements
                    }
                    
                    // Send to integrations (Zapier, Webhooks)
                    await SendToIntegrations(request.CampaignId, request.ConversionData, userAgent, ipAddress);
                    break;
                case "close":
                    // Track popup close events
                    break;
                default:
                    return BadRequest(new { error = "Invalid event type" });
            }

            return Ok(new { success = true, message = $"{request.EventType} tracked successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> TrackBatchEvents([FromBody] TrackingBatchRequest request)
    {
        try
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            int processedCount = 0;

            foreach (var evt in request.Events ?? new List<TrackingEventRequest>())
            {
                try
                {
                    switch (evt.EventType?.ToLower())
                    {
                        case "impression":
                            await _analyticsService.RecordPopupViewAsync(evt.CampaignId, userAgent, ipAddress);
                            break;
                        case "click":
                            await _analyticsService.RecordPopupClickAsync(evt.CampaignId, userAgent, ipAddress);
                            break;
                        case "conversion":
                            await _analyticsService.RecordPopupConversionAsync(evt.CampaignId, userAgent, ipAddress);
                            // Send to integrations
                            await SendToIntegrations(evt.CampaignId, evt.ConversionData, userAgent, ipAddress);
                            break;
                    }
                    processedCount++;
                }
                catch
                {
                    // Continue processing other events
                }
            }

            return Ok(new { success = true, processed = processedCount, total = request.Events?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    private async Task SendToIntegrations(int popupId, Dictionary<string, object>? conversionData, string? userAgent, string? ipAddress)
    {
        try
        {
            var popup = await _context.Popups.FindAsync(popupId);
            if (popup == null || string.IsNullOrEmpty(popup.TargetingRules))
                return;

            // Parse targeting rules to get integration configs
            var targetingRules = System.Text.Json.JsonDocument.Parse(popup.TargetingRules);
            
            // Prepare payload
            var payload = new
            {
                popupId = popupId,
                popupName = popup.Name,
                timestamp = DateTime.UtcNow,
                userAgent = userAgent,
                ipAddress = ipAddress,
                conversionData = conversionData ?? new Dictionary<string, object>()
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Check for Zapier integration
            if (targetingRules.RootElement.TryGetProperty("zapier", out var zapierConfig))
            {
                if (zapierConfig.TryGetProperty("enabled", out var zapierEnabled) && zapierEnabled.GetBoolean())
                {
                    if (zapierConfig.TryGetProperty("webhookUrl", out var webhookUrl))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            await httpClient.PostAsync(webhookUrl.GetString(), content);
                        }
                        catch
                        {
                            // Log error but don't fail the conversion
                        }
                    }
                }
            }

            // Check for custom webhook integration
            if (targetingRules.RootElement.TryGetProperty("webhook", out var webhookConfig))
            {
                if (webhookConfig.TryGetProperty("enabled", out var webhookEnabled) && webhookEnabled.GetBoolean())
                {
                    if (webhookConfig.TryGetProperty("url", out var webhookUrlProp))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            
                            // Get method (default POST)
                            var method = webhookConfig.TryGetProperty("method", out var methodProp) 
                                ? methodProp.GetString() ?? "POST" 
                                : "POST";

                            // Add custom headers if provided
                            if (webhookConfig.TryGetProperty("headers", out var headersProp))
                            {
                                var headersText = headersProp.GetString();
                                if (!string.IsNullOrEmpty(headersText))
                                {
                                    foreach (var line in headersText.Split('\n'))
                                    {
                                        var parts = line.Split(':', 2);
                                        if (parts.Length == 2)
                                        {
                                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                                                parts[0].Trim(), 
                                                parts[1].Trim()
                                            );
                                        }
                                    }
                                }
                            }

                            var request = new HttpRequestMessage(
                                new HttpMethod(method), 
                                webhookUrlProp.GetString()
                            )
                            {
                                Content = content
                            };

                            await httpClient.SendAsync(request);
                        }
                        catch
                        {
                            // Log error but don't fail the conversion
                        }
                    }
                }
            }
        }
        catch
        {
            // Don't fail the conversion if webhook posting fails
        }
    }
}

public class PopupEventRequest
{
    public int TenantId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}

public class PopupConversionRequest : PopupEventRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}

public class PopupApiResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string TargetingRules { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public int DelayMs { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public bool ShowOnMobile { get; set; }
    public bool ShowOnDesktop { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TrackingEventRequest
{
    public int TenantId { get; set; }
    public int CampaignId { get; set; }
    public string? VariationId { get; set; }
    public string EventType { get; set; } = string.Empty; // impression, click, conversion, close
    public Dictionary<string, object>? Metadata { get; set; }
    public Dictionary<string, object>? ConversionData { get; set; }
    public long Timestamp { get; set; }
}

public class TrackingBatchRequest
{
    public int TenantId { get; set; }
    public List<TrackingEventRequest>? Events { get; set; }
}