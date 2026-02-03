using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Notification_Application.Services;
using System.Text.Json;

namespace Notification_Application.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PixelController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PixelController> _logger;
    private readonly IPipelineService _pipelineService;

    public PixelController(ApplicationDbContext context, ILogger<PixelController> logger, IPipelineService pipelineService)
    {
        _context = context;
        _logger = logger;
        _pipelineService = pipelineService;
    }

    // POST: api/Pixel/Track
    [HttpPost("Track")]
    public async Task<IActionResult> Track([FromBody] PixelTrackingData data)
    {
        try
        {
            if (string.IsNullOrEmpty(data.TenantKey))
            {
                return BadRequest("Tenant key is required");
            }

            // Find tenant by API key, tracking code, or website-specific tracking key
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.ApiKey == data.TenantKey || t.TrackingCode == data.TenantKey);

            // If not found by tenant keys, check if it's a website-specific tracking key
            if (tenant == null)
            {
                var website = await _context.AllowedWebsites
                    .Include(w => w.Tenant)
                    .FirstOrDefaultAsync(w => w.TrackingKey == data.TenantKey && w.IsActive);
                
                if (website != null)
                {
                    tenant = website.Tenant;
                }
            }

            if (tenant == null)
            {
                return NotFound("Invalid tenant key");
            }

            // Find lead by email or create anonymous session
            Lead? lead = null;
            if (!string.IsNullOrEmpty(data.Email))
            {
                lead = await _context.Leads
                    .FirstOrDefaultAsync(l => l.TenantId == tenant.Id && l.Email == data.Email);
            }

            if (lead != null)
            {
                // Update lead engagement metrics
                lead.PageViews++;
                lead.LastVisitAt = DateTime.UtcNow;
                
                if (!lead.FirstVisitAt.HasValue)
                {
                    lead.FirstVisitAt = DateTime.UtcNow;
                    lead.TotalVisits = 1;
                }
                else
                {
                    // Check if this is a new session (more than 30 minutes since last visit)
                    if (lead.LastVisitAt.HasValue && (DateTime.UtcNow - lead.LastVisitAt.Value).TotalMinutes > 30)
                    {
                        lead.TotalVisits++;
                    }
                }

                // Update average time on site
                if (data.TimeOnPage.HasValue && data.TimeOnPage.Value > 0)
                {
                    if (lead.AverageTimeOnSite.HasValue)
                    {
                        lead.AverageTimeOnSite = (lead.AverageTimeOnSite.Value + data.TimeOnPage.Value) / 2;
                    }
                    else
                    {
                        lead.AverageTimeOnSite = data.TimeOnPage.Value;
                    }
                }

                // Update location data
                if (!string.IsNullOrEmpty(data.City)) lead.City = data.City;
                if (!string.IsNullOrEmpty(data.State)) lead.State = data.State;
                if (!string.IsNullOrEmpty(data.Country)) lead.Country = data.Country;

                // Create activity record
                var activity = new LeadActivity
                {
                    LeadId = lead.Id,
                    TenantId = tenant.Id,
                    Type = GetActivityType(data.EventType),
                    Title = GetActivityTitle(data.EventType, data.PageTitle ?? data.PageUrl),
                    Description = data.EventData,
                    PageUrl = data.PageUrl,
                    PageTitle = data.PageTitle,
                    TimeOnPage = data.TimeOnPage,
                    DeviceType = data.DeviceType,
                    Browser = data.Browser,
                    Location = $"{data.City}, {data.State}, {data.Country}".Trim(new[] { ',', ' ' }),
                    Metadata = data.CustomData,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeadActivities.Add(activity);

                // Update lead score based on activity
                await UpdateLeadScore(lead, data.EventType);

                await _context.SaveChangesAsync();

                return Ok(new { success = true, leadId = lead.Id });
            }
            else
            {
                // For anonymous visitors, we could still log the activity
                // and associate it later when they become a lead
                _logger.LogInformation($"Anonymous activity tracked: {data.EventType} on {data.PageUrl}");
                
                return Ok(new { success = true, message = "Anonymous activity tracked" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking pixel data");
            return StatusCode(500, "Error tracking data");
        }
    }

    // POST: api/Pixel/IdentifyLead
    [HttpPost("IdentifyLead")]
    public async Task<IActionResult> IdentifyLead([FromBody] LeadIdentificationData data)
    {
        try
        {
            if (string.IsNullOrEmpty(data.TenantKey) || string.IsNullOrEmpty(data.Email))
            {
                return BadRequest("Tenant key and email are required");
            }

            // Find tenant by API key, tracking code, or website-specific tracking key
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.ApiKey == data.TenantKey || t.TrackingCode == data.TenantKey);

            // If not found by tenant keys, check if it's a website-specific tracking key
            if (tenant == null)
            {
                var website = await _context.AllowedWebsites
                    .Include(w => w.Tenant)
                    .FirstOrDefaultAsync(w => w.TrackingKey == data.TenantKey && w.IsActive);
                
                if (website != null)
                {
                    tenant = website.Tenant;
                }
            }

            if (tenant == null)
            {
                return NotFound("Invalid tenant key");
            }

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.TenantId == tenant.Id && l.Email == data.Email);

            if (lead == null)
            {
                // Create new lead
                lead = new Lead
                {
                    TenantId = tenant.Id,
                    Email = data.Email,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Phone = data.Phone,
                    Company = data.Company,
                    JobTitle = data.JobTitle,
                    Website = data.Website,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Source = data.PageUrl,
                    LeadSource = data.Source,
                    FirstVisitAt = DateTime.UtcNow,
                    LastVisitAt = DateTime.UtcNow,
                    TotalVisits = 1,
                    PageViews = 1,
                    CapturedAt = DateTime.UtcNow
                };

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                // Auto-assign lead to pipeline
                await _pipelineService.AssignLeadToPipelineAsync(lead, tenant.Id);

                // Create lead created activity
                var activity = new LeadActivity
                {
                    LeadId = lead.Id,
                    TenantId = tenant.Id,
                    Type = ActivityType.LeadCreated,
                    Title = "Lead identified",
                    Description = $"Lead identified from {data.PageUrl}",
                    PageUrl = data.PageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeadActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing lead
                if (!string.IsNullOrEmpty(data.FirstName)) lead.FirstName = data.FirstName;
                if (!string.IsNullOrEmpty(data.LastName)) lead.LastName = data.LastName;
                if (!string.IsNullOrEmpty(data.Phone)) lead.Phone = data.Phone;
                if (!string.IsNullOrEmpty(data.Company)) lead.Company = data.Company;
                if (!string.IsNullOrEmpty(data.JobTitle)) lead.JobTitle = data.JobTitle;
                if (!string.IsNullOrEmpty(data.Website)) lead.Website = data.Website;

                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, leadId = lead.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying lead");
            return StatusCode(500, "Error identifying lead");
        }
    }

    private ActivityType GetActivityType(string eventType)
    {
        return eventType?.ToLower() switch
        {
            "pageview" => ActivityType.PageView,
            "popupview" => ActivityType.PopupView,
            "popupinteraction" => ActivityType.PopupInteraction,
            "buttonclick" => ActivityType.ButtonClick,
            "videowatch" => ActivityType.VideoWatch,
            "filedownload" => ActivityType.FileDownload,
            "productview" => ActivityType.ProductView,
            "cartabandonment" => ActivityType.CartAbandonment,
            "purchase" => ActivityType.Purchase,
            "formsubmitted" => ActivityType.FormSubmitted,
            "emailopened" => ActivityType.EmailOpened,
            "emailclicked" => ActivityType.EmailClicked,
            _ => ActivityType.Custom
        };
    }

    private string GetActivityTitle(string eventType, string? context)
    {
        var type = eventType?.ToLower();
        return type switch
        {
            "pageview" => $"Viewed page: {context}",
            "popupview" => $"Viewed popup: {context}",
            "popupinteraction" => $"Interacted with popup: {context}",
            "buttonclick" => $"Clicked button: {context}",
            "videowatch" => $"Watched video: {context}",
            "filedownload" => $"Downloaded file: {context}",
            "productview" => $"Viewed product: {context}",
            "cartabandonment" => "Abandoned cart",
            "purchase" => $"Made purchase: {context}",
            "formsubmitted" => "Submitted form",
            "emailopened" => "Opened email",
            "emailclicked" => "Clicked email link",
            _ => $"{eventType}: {context}"
        };
    }

    private async System.Threading.Tasks.Task UpdateLeadScore(Lead lead, string eventType)
    {
        int scoreChange = eventType?.ToLower() switch
        {
            "pageview" => 1,
            "popupview" => 2,
            "popupinteraction" => 5,
            "buttonclick" => 3,
            "videowatch" => 10,
            "filedownload" => 15,
            "productview" => 5,
            "cartabandonment" => -5,
            "purchase" => 50,
            "formsubmitted" => 20,
            "emailopened" => 3,
            "emailclicked" => 7,
            _ => 0
        };

        if (scoreChange != 0)
        {
            lead.Score += scoreChange;
            lead.LastScoreUpdate = DateTime.UtcNow;

            var scoreRecord = new LeadScore
            {
                LeadId = lead.Id,
                Score = lead.Score,
                Reason = $"Score changed by {scoreChange} due to {eventType}",
                CalculatedAt = DateTime.UtcNow
            };

            _context.LeadScores.Add(scoreRecord);
        }
    }
}

public class PixelTrackingData
{
    public string TenantKey { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string EventType { get; set; } = "pageview"; // pageview, click, purchase, etc.
    public string? PageUrl { get; set; }
    public string? PageTitle { get; set; }
    public int? TimeOnPage { get; set; } // seconds
    public string? DeviceType { get; set; } // mobile, tablet, desktop
    public string? Browser { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? EventData { get; set; } // Additional event-specific data
    public string? CustomData { get; set; } // JSON string for custom tracking data
}

public class LeadIdentificationData
{
    public string TenantKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Website { get; set; }
    public string? PageUrl { get; set; }
    public string? Source { get; set; }
}
