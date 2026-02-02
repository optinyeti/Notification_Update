using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User")]
public class IntegrationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(
        ApplicationDbContext context, 
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<IntegrationController> logger)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Integration
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get tenant's subscription plan
        var tenant = await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

        // Get all connected integrations for this tenant
        var connectedIntegrations = await _context.Integrations
            .Where(i => i.TenantId == user.TenantId && i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

        // Get all available integrations from catalog
        var catalog = IntegrationCatalog.GetAll();

        var viewModel = new IntegrationIndexViewModel
        {
            ConnectedIntegrations = connectedIntegrations,
            AvailableCrmIntegrations = catalog.Where(c => c.Category == IntegrationCategory.CRM).ToList(),
            AvailableEmailIntegrations = catalog.Where(c => c.Category == IntegrationCategory.EmailMarketing).ToList(),
            AvailableAutomationIntegrations = catalog.Where(c => c.Category == IntegrationCategory.Automation).ToList(),
            AvailableAnalyticsIntegrations = catalog.Where(c => c.Category == IntegrationCategory.Analytics).ToList(),
            AvailableCommunicationIntegrations = catalog.Where(c => c.Category == IntegrationCategory.Communication).ToList(),
            AvailableEcommerceIntegrations = catalog.Where(c => c.Category == IntegrationCategory.eCommerce).ToList(),
            CustomIntegrations = catalog.Where(c => c.Category == IntegrationCategory.Custom).ToList(),
            PlanId = tenant?.SubscriptionPlanId ?? 1,
            TenantId = user.TenantId
        };

        return View(viewModel);
    }

    // GET: Integration/Configure/{type}
    public async Task<IActionResult> Configure(IntegrationType type)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var info = IntegrationCatalog.GetInfo(type);
        if (info == null)
        {
            TempData["Error"] = "Integration type not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check if this integration type is already configured
        var existing = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == user.TenantId && i.Type == type && i.IsActive);

        // Get recent logs if integration exists
        var recentLogs = new List<IntegrationLog>();
        if (existing != null)
        {
            recentLogs = await _context.IntegrationLogs
                .Where(l => l.IntegrationId == existing.Id)
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        var viewModel = new ConfigureIntegrationViewModel
        {
            IntegrationType = type,
            Info = info,
            ExistingIntegration = existing,
            IsNew = existing == null,
            RecentLogs = recentLogs
        };

        return View(viewModel);
    }

    // POST: Integration/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SaveIntegrationRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            Integration integration;

            if (request.IntegrationId.HasValue)
            {
                // Update existing
                integration = await _context.Integrations
                    .FirstOrDefaultAsync(i => i.Id == request.IntegrationId && i.TenantId == user.TenantId);

                if (integration == null)
                {
                    TempData["Error"] = "Integration not found.";
                    return RedirectToAction(nameof(Index));
                }

                integration.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var info = IntegrationCatalog.GetInfo(request.IntegrationType);
                if (info == null)
                {
                    TempData["Error"] = "Invalid integration type.";
                    return RedirectToAction(nameof(Index));
                }

                integration = new Integration
                {
                    TenantId = user.TenantId,
                    Type = request.IntegrationType,
                    Name = info.Name,
                    Description = info.Description,
                    Category = info.Category,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Integrations.Add(integration);
            }

            // Update fields based on type
            integration.ApiKey = request.ApiKey;
            integration.ApiSecret = request.ApiSecret;
            integration.WebhookUrl = request.WebhookUrl;
            integration.ExternalListId = request.ListId;
            integration.ExternalAccountId = request.AccountId;
            integration.SyncLeads = request.SyncLeads;
            integration.SyncForms = request.SyncForms;
            integration.SyncEvents = request.SyncEvents;
            integration.IsActive = true;

            // Set specific settings based on type
            SetIntegrationSettings(integration, request);

            // Test connection if API key provided
            if (!string.IsNullOrEmpty(request.ApiKey) || !string.IsNullOrEmpty(request.WebhookUrl))
            {
                integration.IsConnected = await TestIntegrationConnection(integration);
            }

            await _context.SaveChangesAsync();

            // Log the configuration
            var log = new IntegrationLog
            {
                IntegrationId = integration.Id,
                TenantId = user.TenantId,
                Type = IntegrationLogType.Sync,
                Message = integration.IsConnected ? "Integration configured successfully" : "Integration configured (connection pending)",
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.IntegrationLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = integration.IsConnected 
                ? $"{integration.Name} connected successfully!" 
                : $"{integration.Name} configuration saved. Please verify your credentials.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving integration");
            TempData["Error"] = "An error occurred while saving the integration.";
            return RedirectToAction(nameof(Configure), new { type = request.IntegrationType });
        }
    }

    // POST: Integration/Disconnect/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disconnect(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == user.TenantId);

        if (integration == null)
        {
            TempData["Error"] = "Integration not found.";
            return RedirectToAction(nameof(Index));
        }

        integration.IsConnected = false;
        integration.IsActive = false;
        integration.UpdatedAt = DateTime.UtcNow;

        // Log the disconnection
        var log = new IntegrationLog
        {
            IntegrationId = integration.Id,
            TenantId = user.TenantId,
            Type = IntegrationLogType.Sync,
            Message = "Integration disconnected",
            IsSuccess = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.IntegrationLogs.Add(log);

        await _context.SaveChangesAsync();

        TempData["Success"] = $"{integration.Name} disconnected successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Integration/Test/{id}
    [HttpPost]
    public async Task<IActionResult> Test(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == user.TenantId);

        if (integration == null)
        {
            return Json(new { success = false, message = "Integration not found" });
        }

        try
        {
            var isConnected = await TestIntegrationConnection(integration);
            integration.IsConnected = isConnected;
            integration.UpdatedAt = DateTime.UtcNow;

            if (!isConnected)
            {
                integration.LastErrorAt = DateTime.UtcNow;
                integration.LastError = "Connection test failed";
            }

            // Log the test
            var log = new IntegrationLog
            {
                IntegrationId = integration.Id,
                TenantId = user.TenantId,
                Type = IntegrationLogType.Test,
                Message = isConnected ? "Connection test successful" : "Connection test failed",
                IsSuccess = isConnected,
                CreatedAt = DateTime.UtcNow
            };
            _context.IntegrationLogs.Add(log);

            await _context.SaveChangesAsync();

            return Json(new { 
                success = isConnected, 
                message = isConnected ? "Connection successful!" : "Connection failed. Please check your credentials."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing integration {IntegrationId}", id);
            return Json(new { success = false, message = "An error occurred while testing the connection." });
        }
    }

    // GET: Integration/Logs/{id}
    public async Task<IActionResult> Logs(int id, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == user.TenantId);

        if (integration == null)
        {
            TempData["Error"] = "Integration not found.";
            return RedirectToAction(nameof(Index));
        }

        int pageSize = 50;
        var logs = await _context.IntegrationLogs
            .Where(l => l.IntegrationId == id)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalLogs = await _context.IntegrationLogs.CountAsync(l => l.IntegrationId == id);

        ViewBag.Integration = integration;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

        return View(logs);
    }

    // API Endpoints for AJAX calls

    // POST: Integration/Api/SyncLead
    [HttpPost]
    public async Task<IActionResult> SyncLead(int integrationId, int leadId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.Id == integrationId && i.TenantId == user.TenantId && i.IsConnected);

        if (integration == null)
        {
            return Json(new { success = false, message = "Integration not found or not connected" });
        }

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return Json(new { success = false, message = "Lead not found" });
        }

        try
        {
            var result = await PushLeadToIntegration(integration, lead);

            // Log the sync
            var log = new IntegrationLog
            {
                IntegrationId = integration.Id,
                TenantId = user.TenantId,
                Type = IntegrationLogType.LeadPush,
                LeadId = lead.Id,
                Message = result.Success ? "Lead synced successfully" : result.Message,
                IsSuccess = result.Success,
                CreatedAt = DateTime.UtcNow
            };
            _context.IntegrationLogs.Add(log);

            if (result.Success)
            {
                integration.SyncCount++;
                integration.LastSyncAt = DateTime.UtcNow;
            }
            else
            {
                integration.ErrorCount++;
                integration.LastErrorAt = DateTime.UtcNow;
                integration.LastError = result.Message;
            }

            await _context.SaveChangesAsync();

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing lead {LeadId} to integration {IntegrationId}", leadId, integrationId);
            return Json(new { success = false, message = "An error occurred while syncing the lead." });
        }
    }

    // Helper methods

    private void SetIntegrationSettings(Integration integration, SaveIntegrationRequest request)
    {
        switch (integration.Type)
        {
            case IntegrationType.Mailchimp:
                integration.SetSettings(new MailchimpSettings
                {
                    ListId = request.ListId,
                    DataCenter = ExtractMailchimpDataCenter(request.ApiKey),
                    DoubleOptIn = request.DoubleOptIn
                });
                break;

            case IntegrationType.HubSpot:
                integration.SetSettings(new HubSpotSettings
                {
                    PortalId = request.AccountId,
                    CreateContacts = true,
                    CreateDeals = request.CreateDeals,
                    LeadSource = "Popup Manager"
                });
                break;

            case IntegrationType.ActiveCampaign:
                integration.SetSettings(new ActiveCampaignSettings
                {
                    AccountUrl = request.AccountUrl,
                    ListId = int.TryParse(request.ListId, out var listId) ? listId : null,
                    DoubleOptIn = request.DoubleOptIn
                });
                break;

            case IntegrationType.Brevo:
                integration.SetSettings(new BrevoSettings
                {
                    ListId = int.TryParse(request.ListId, out var brevoListId) ? brevoListId : null,
                    DoubleOptIn = request.DoubleOptIn
                });
                break;

            case IntegrationType.Zapier:
            case IntegrationType.Make:
                integration.SetSettings(new ZapierSettings
                {
                    WebhookUrl = request.WebhookUrl,
                    TriggerEvents = new List<string> { "lead_created", "form_submitted" },
                    IncludeMetadata = true
                });
                break;

            case IntegrationType.n8n:
                integration.SetSettings(new n8nSettings
                {
                    WebhookUrl = request.WebhookUrl,
                    TriggerEvents = new List<string> { "lead_created", "form_submitted" },
                    AuthHeaderName = request.AuthHeaderName,
                    AuthHeaderValue = request.AuthHeaderValue
                });
                break;

            case IntegrationType.GoogleAnalytics4:
                integration.SetSettings(new GA4Settings
                {
                    MeasurementId = request.MeasurementId,
                    ApiSecret = request.ApiSecret,
                    TrackPageViews = true,
                    TrackFormSubmissions = true,
                    TrackPopupViews = true,
                    TrackConversions = true
                });
                break;

            case IntegrationType.Webhook:
                integration.SetSettings(new WebhookSettings
                {
                    Url = request.WebhookUrl,
                    Method = request.WebhookMethod ?? "POST",
                    RetryOnFailure = true,
                    TriggerEvents = new List<string> { "lead_created" }
                });
                break;

            // Add more cases as needed
        }
    }

    private string? ExtractMailchimpDataCenter(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey)) return null;
        var parts = apiKey.Split('-');
        return parts.Length == 2 ? parts[1] : null;
    }

    private async Task<bool> TestIntegrationConnection(Integration integration)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            switch (integration.Type)
            {
                case IntegrationType.Mailchimp:
                    return await TestMailchimpConnection(httpClient, integration);

                case IntegrationType.HubSpot:
                    return await TestHubSpotConnection(httpClient, integration);

                case IntegrationType.ActiveCampaign:
                    return await TestActiveCampaignConnection(httpClient, integration);

                case IntegrationType.Brevo:
                    return await TestBrevoConnection(httpClient, integration);

                case IntegrationType.Zapier:
                case IntegrationType.Make:
                case IntegrationType.n8n:
                case IntegrationType.Webhook:
                    // For webhook-based integrations, just verify the URL is valid
                    return !string.IsNullOrEmpty(integration.WebhookUrl) 
                        && Uri.TryCreate(integration.WebhookUrl, UriKind.Absolute, out _);

                case IntegrationType.GoogleAnalytics4:
                    // GA4 doesn't have a simple test endpoint, assume valid if measurement ID provided
                    var ga4Settings = integration.GetSettings<GA4Settings>();
                    return !string.IsNullOrEmpty(ga4Settings?.MeasurementId);

                default:
                    return true; // Assume connected for other types
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed for integration {IntegrationId}", integration.Id);
            return false;
        }
    }

    private async Task<bool> TestMailchimpConnection(HttpClient httpClient, Integration integration)
    {
        if (string.IsNullOrEmpty(integration.ApiKey)) return false;

        var settings = integration.GetSettings<MailchimpSettings>();
        var dataCenter = settings?.DataCenter ?? ExtractMailchimpDataCenter(integration.ApiKey);
        if (string.IsNullOrEmpty(dataCenter)) return false;

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.ApiKey);

        var response = await httpClient.GetAsync($"https://{dataCenter}.api.mailchimp.com/3.0/ping");
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> TestHubSpotConnection(HttpClient httpClient, Integration integration)
    {
        if (string.IsNullOrEmpty(integration.AccessToken) && string.IsNullOrEmpty(integration.ApiKey)) 
            return false;

        var token = integration.AccessToken ?? integration.ApiKey;
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync("https://api.hubapi.com/crm/v3/objects/contacts?limit=1");
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> TestActiveCampaignConnection(HttpClient httpClient, Integration integration)
    {
        if (string.IsNullOrEmpty(integration.ApiKey)) return false;

        var settings = integration.GetSettings<ActiveCampaignSettings>();
        if (string.IsNullOrEmpty(settings?.AccountUrl)) return false;

        httpClient.DefaultRequestHeaders.Add("Api-Token", integration.ApiKey);

        var response = await httpClient.GetAsync($"{settings.AccountUrl}/api/3/users/me");
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> TestBrevoConnection(HttpClient httpClient, Integration integration)
    {
        if (string.IsNullOrEmpty(integration.ApiKey)) return false;

        httpClient.DefaultRequestHeaders.Add("api-key", integration.ApiKey);

        var response = await httpClient.GetAsync("https://api.brevo.com/v3/account");
        return response.IsSuccessStatusCode;
    }

    private async Task<IntegrationSyncResult> PushLeadToIntegration(Integration integration, Lead lead)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            switch (integration.Type)
            {
                case IntegrationType.Zapier:
                case IntegrationType.Make:
                case IntegrationType.n8n:
                case IntegrationType.Webhook:
                    return await PushToWebhook(httpClient, integration, lead);

                case IntegrationType.Mailchimp:
                    return await PushToMailchimp(httpClient, integration, lead);

                case IntegrationType.HubSpot:
                    return await PushToHubSpot(httpClient, integration, lead);

                case IntegrationType.Brevo:
                    return await PushToBrevo(httpClient, integration, lead);

                case IntegrationType.ActiveCampaign:
                    return await PushToActiveCampaign(httpClient, integration, lead);

                default:
                    return new IntegrationSyncResult { Success = false, Message = "Integration type not supported for lead sync" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing lead to integration");
            return new IntegrationSyncResult { Success = false, Message = ex.Message };
        }
    }

    private async Task<IntegrationSyncResult> PushToWebhook(HttpClient httpClient, Integration integration, Lead lead)
    {
        var settings = integration.GetSettings<WebhookSettings>();
        if (string.IsNullOrEmpty(settings?.Url) && string.IsNullOrEmpty(integration.WebhookUrl))
        {
            return new IntegrationSyncResult { Success = false, Message = "Webhook URL not configured" };
        }

        var url = settings?.Url ?? integration.WebhookUrl;

        var payload = new
        {
            @event = "lead_created",
            timestamp = DateTime.UtcNow,
            data = new
            {
                id = lead.Id,
                email = lead.Email,
                firstName = lead.FirstName,
                lastName = lead.LastName,
                phone = lead.Phone,
                company = lead.Company,
                source = lead.LeadSource,
                capturedAt = lead.CapturedAt,
                utmSource = lead.UtmSource,
                utmMedium = lead.UtmMedium,
                utmCampaign = lead.UtmCampaign,
                customFields = lead.CustomFields
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Add custom headers for n8n
        if (integration.Type == IntegrationType.n8n)
        {
            var n8nSettings = integration.GetSettings<n8nSettings>();
            if (!string.IsNullOrEmpty(n8nSettings?.AuthHeaderName) && !string.IsNullOrEmpty(n8nSettings?.AuthHeaderValue))
            {
                httpClient.DefaultRequestHeaders.Add(n8nSettings.AuthHeaderName, n8nSettings.AuthHeaderValue);
            }
        }

        var response = await httpClient.PostAsync(url, content);

        return new IntegrationSyncResult
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Lead sent successfully" : $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
            StatusCode = (int)response.StatusCode
        };
    }

    private async Task<IntegrationSyncResult> PushToMailchimp(HttpClient httpClient, Integration integration, Lead lead)
    {
        if (string.IsNullOrEmpty(lead.Email))
        {
            return new IntegrationSyncResult { Success = false, Message = "Lead has no email address" };
        }

        var settings = integration.GetSettings<MailchimpSettings>();
        var dataCenter = settings?.DataCenter ?? ExtractMailchimpDataCenter(integration.ApiKey);
        var listId = settings?.ListId ?? integration.ExternalListId;

        if (string.IsNullOrEmpty(dataCenter) || string.IsNullOrEmpty(listId))
        {
            return new IntegrationSyncResult { Success = false, Message = "Mailchimp not properly configured" };
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.ApiKey);

        var payload = new
        {
            email_address = lead.Email,
            status = settings?.DoubleOptIn == true ? "pending" : "subscribed",
            merge_fields = new
            {
                FNAME = lead.FirstName ?? "",
                LNAME = lead.LastName ?? "",
                PHONE = lead.Phone ?? "",
                COMPANY = lead.Company ?? ""
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            $"https://{dataCenter}.api.mailchimp.com/3.0/lists/{listId}/members",
            content);

        var responseContent = await response.Content.ReadAsStringAsync();

        // Mailchimp returns 400 if member already exists
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && responseContent.Contains("already a list member"))
        {
            return new IntegrationSyncResult { Success = true, Message = "Contact already exists in Mailchimp" };
        }

        return new IntegrationSyncResult
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Added to Mailchimp" : $"Mailchimp error: {responseContent}",
            StatusCode = (int)response.StatusCode
        };
    }

    private async Task<IntegrationSyncResult> PushToHubSpot(HttpClient httpClient, Integration integration, Lead lead)
    {
        if (string.IsNullOrEmpty(lead.Email))
        {
            return new IntegrationSyncResult { Success = false, Message = "Lead has no email address" };
        }

        var token = integration.AccessToken ?? integration.ApiKey;
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            properties = new
            {
                email = lead.Email,
                firstname = lead.FirstName ?? "",
                lastname = lead.LastName ?? "",
                phone = lead.Phone ?? "",
                company = lead.Company ?? "",
                website = lead.Website ?? "",
                jobtitle = lead.JobTitle ?? "",
                city = lead.City ?? "",
                state = lead.State ?? "",
                country = lead.Country ?? "",
                hs_lead_status = "NEW"
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            "https://api.hubapi.com/crm/v3/objects/contacts",
            content);

        var responseContent = await response.Content.ReadAsStringAsync();

        // HubSpot returns 409 if contact exists
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return new IntegrationSyncResult { Success = true, Message = "Contact already exists in HubSpot" };
        }

        return new IntegrationSyncResult
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Added to HubSpot" : $"HubSpot error: {responseContent}",
            StatusCode = (int)response.StatusCode
        };
    }

    private async Task<IntegrationSyncResult> PushToBrevo(HttpClient httpClient, Integration integration, Lead lead)
    {
        if (string.IsNullOrEmpty(lead.Email))
        {
            return new IntegrationSyncResult { Success = false, Message = "Lead has no email address" };
        }

        var settings = integration.GetSettings<BrevoSettings>();

        httpClient.DefaultRequestHeaders.Add("api-key", integration.ApiKey);

        var listIds = settings?.ListId.HasValue == true 
            ? new List<int> { settings.ListId.Value } 
            : new List<int>();

        var payload = new
        {
            email = lead.Email,
            attributes = new
            {
                FIRSTNAME = lead.FirstName ?? "",
                LASTNAME = lead.LastName ?? "",
                SMS = lead.Phone ?? ""
            },
            listIds = listIds
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://api.brevo.com/v3/contacts", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Brevo returns 400 if contact exists
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && responseContent.Contains("already exist"))
        {
            return new IntegrationSyncResult { Success = true, Message = "Contact already exists in Brevo" };
        }

        return new IntegrationSyncResult
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Added to Brevo" : $"Brevo error: {responseContent}",
            StatusCode = (int)response.StatusCode
        };
    }

    private async Task<IntegrationSyncResult> PushToActiveCampaign(HttpClient httpClient, Integration integration, Lead lead)
    {
        if (string.IsNullOrEmpty(lead.Email))
        {
            return new IntegrationSyncResult { Success = false, Message = "Lead has no email address" };
        }

        var settings = integration.GetSettings<ActiveCampaignSettings>();
        if (string.IsNullOrEmpty(settings?.AccountUrl))
        {
            return new IntegrationSyncResult { Success = false, Message = "ActiveCampaign not properly configured" };
        }

        httpClient.DefaultRequestHeaders.Add("Api-Token", integration.ApiKey);

        var payload = new
        {
            contact = new
            {
                email = lead.Email,
                firstName = lead.FirstName ?? "",
                lastName = lead.LastName ?? "",
                phone = lead.Phone ?? ""
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{settings.AccountUrl}/api/3/contacts", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return new IntegrationSyncResult
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Added to ActiveCampaign" : $"ActiveCampaign error: {responseContent}",
            StatusCode = (int)response.StatusCode
        };
    }
}

// View Models
public class IntegrationIndexViewModel
{
    public List<Integration> ConnectedIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableCrmIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableEmailIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableAutomationIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableAnalyticsIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableCommunicationIntegrations { get; set; } = new();
    public List<IntegrationInfo> AvailableEcommerceIntegrations { get; set; } = new();
    public List<IntegrationInfo> CustomIntegrations { get; set; } = new();
    public int PlanId { get; set; }
    public int TenantId { get; set; }
}

public class ConfigureIntegrationViewModel
{
    public IntegrationType IntegrationType { get; set; }
    public IntegrationInfo Info { get; set; } = null!;
    public Integration? ExistingIntegration { get; set; }
    public bool IsNew { get; set; }
    public List<IntegrationLog> RecentLogs { get; set; } = new();
}

public class SaveIntegrationRequest
{
    public int? IntegrationId { get; set; }
    public IntegrationType IntegrationType { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookUrl { get; set; }
    public string? ListId { get; set; }
    public string? AccountId { get; set; }
    public string? AccountUrl { get; set; }
    public bool SyncLeads { get; set; } = true;
    public bool SyncForms { get; set; } = true;
    public bool SyncEvents { get; set; } = false;
    public bool DoubleOptIn { get; set; } = false;
    public bool CreateDeals { get; set; } = false;
    public string? MeasurementId { get; set; }
    public string? AuthHeaderName { get; set; }
    public string? AuthHeaderValue { get; set; }
    public string? WebhookMethod { get; set; }
}

public class IntegrationSyncResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
    public string? ExternalId { get; set; }
}