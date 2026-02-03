using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace Notification_Application.Controllers.Api
{
    [ApiController]
    [Route("api/twilio")]
    public class TwilioWebhookController : TwilioController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TwilioWebhookController> _logger;

        public TwilioWebhookController(ApplicationDbContext context, ILogger<TwilioWebhookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("voice")]
        public async Task<IActionResult> Voice([FromForm] string From, [FromForm] string To, [FromForm] string CallSid,
            [FromForm] string? CallerCity, [FromForm] string? CallerState, [FromForm] string? CallerCountry, [FromForm] string? CallerZip)
        {
            try
            {
                // Find the phone number in our database
                var phoneNumber = await _context.PhoneNumbers
                    .FirstOrDefaultAsync(p => p.Number == To && p.IsActive);

                if (phoneNumber == null)
                {
                    _logger.LogWarning($"Phone number not found or inactive: {To}");
                    var errorResponse = new VoiceResponse();
                    errorResponse.Say("This number is no longer in service.");
                    errorResponse.Hangup();
                    return TwiML(errorResponse);
                }

                // Create call record
                var phoneCall = new PhoneCall
                {
                    PhoneNumberId = phoneNumber.Id,
                    CallSid = CallSid,
                    FromNumber = From,
                    ToNumber = To,
                    ForwardedTo = phoneNumber.ForwardToNumber,
                    Status = CallStatus.Ringing,
                    CallDate = DateTime.UtcNow,
                    CallerCity = CallerCity,
                    CallerState = CallerState,
                    CallerCountry = CallerCountry,
                    CallerZip = CallerZip,
                    UtmSource = phoneNumber.UtmSource,
                    UtmMedium = phoneNumber.UtmMedium,
                    UtmCampaign = phoneNumber.UtmCampaign,
                    UtmTerm = phoneNumber.UtmTerm,
                    UtmContent = phoneNumber.UtmContent
                };

                _context.PhoneCalls.Add(phoneCall);
                await _context.SaveChangesAsync();

                // Forward the call
                var response = new VoiceResponse();
                var dial = new Dial(
                    record: Dial.RecordEnum.RecordFromAnswer,
                    recordingStatusCallback: new Uri($"/api/twilio/recording?callId={phoneCall.Id}", UriKind.Relative),
                    action: new Uri($"/api/twilio/status?callId={phoneCall.Id}", UriKind.Relative)
                );
                dial.Number(phoneNumber.ForwardToNumber);
                response.Append(dial);

                return TwiML(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice call");
                var errorResponse = new VoiceResponse();
                errorResponse.Say("We're experiencing technical difficulties. Please try again later.");
                errorResponse.Hangup();
                return TwiML(errorResponse);
            }
        }

        [HttpPost("status")]
        public async Task<IActionResult> Status([FromQuery] int callId, [FromForm] string CallStatus, 
            [FromForm] string? CallDuration, [FromForm] string? RecordingUrl)
        {
            try
            {
                var phoneCall = await _context.PhoneCalls
                    .Include(p => p.PhoneNumber)
                    .FirstOrDefaultAsync(p => p.Id == callId);

                if (phoneCall == null)
                {
                    _logger.LogWarning($"Phone call not found: {callId}");
                    return Ok();
                }

                // Update call status
                phoneCall.Status = CallStatus switch
                {
                    "ringing" => Models.CallStatus.Ringing,
                    "in-progress" => Models.CallStatus.InProgress,
                    "completed" => Models.CallStatus.Completed,
                    "busy" => Models.CallStatus.Busy,
                    "failed" => Models.CallStatus.Failed,
                    "no-answer" => Models.CallStatus.NoAnswer,
                    "canceled" => Models.CallStatus.Canceled,
                    _ => phoneCall.Status
                };

                if (!string.IsNullOrEmpty(CallDuration) && int.TryParse(CallDuration, out int duration))
                {
                    phoneCall.DurationSeconds = duration;
                    
                    // Calculate cost
                    var minutes = (decimal)duration / 60;
                    phoneCall.Cost = minutes * phoneCall.PhoneNumber!.PerMinuteRate;
                }

                if (!string.IsNullOrEmpty(RecordingUrl))
                {
                    phoneCall.RecordingUrl = RecordingUrl;
                }

                // Update phone number stats
                if (phoneCall.Status == Models.CallStatus.Completed)
                {
                    phoneCall.PhoneNumber!.TotalCalls++;
                    phoneCall.PhoneNumber.TotalMinutes += phoneCall.DurationSeconds / 60;
                    phoneCall.PhoneNumber.TotalCost += phoneCall.Cost;
                    phoneCall.PhoneNumber.LastCallDate = DateTime.UtcNow;

                    // Create a lead from the call if configured
                    await CreateLeadFromCall(phoneCall);
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating call status");
                return Ok(); // Return OK to Twilio even on error
            }
        }

        [HttpPost("recording")]
        public async Task<IActionResult> Recording([FromQuery] int callId, [FromForm] string RecordingUrl)
        {
            try
            {
                var phoneCall = await _context.PhoneCalls.FindAsync(callId);
                if (phoneCall != null)
                {
                    phoneCall.RecordingUrl = RecordingUrl;
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving recording URL");
                return Ok();
            }
        }

        private async System.Threading.Tasks.Task CreateLeadFromCall(PhoneCall phoneCall)
        {
            try
            {
                // Check if lead already exists for this phone number
                var existingLead = await _context.Leads
                    .FirstOrDefaultAsync(l => l.Phone == phoneCall.FromNumber);

                if (existingLead != null)
                {
                    // Link call to existing lead
                    phoneCall.LeadId = existingLead.Id;
                    phoneCall.ConvertedToLead = true;
                    return;
                }

                // Create new lead
                var lead = new Lead
                {
                    TenantId = phoneCall.PhoneNumber!.User!.TenantId,
                    Phone = phoneCall.FromNumber,
                    FirstName = "Phone Lead",
                    LastName = phoneCall.CallerCity ?? "Unknown",
                    Source = $"Call Tracking - {phoneCall.PhoneNumber.FriendlyName}",
                    Status = LeadStatus.New,
                    CapturedAt = DateTime.UtcNow,
                    LastContactedAt = DateTime.UtcNow,
                    UtmSource = phoneCall.UtmSource,
                    UtmMedium = phoneCall.UtmMedium,
                    UtmCampaign = phoneCall.UtmCampaign,
                    UtmTerm = phoneCall.UtmTerm,
                    UtmContent = phoneCall.UtmContent
                };

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                phoneCall.LeadId = lead.Id;
                phoneCall.ConvertedToLead = true;

                // Create activity
                var activity = new LeadActivity
                {
                    TenantId = phoneCall.PhoneNumber!.User!.TenantId,
                    LeadId = lead.Id,
                    Type = ActivityType.Call,
                    Title = "Inbound Phone Call",
                    Description = $"Inbound call from {phoneCall.FromNumber} to {phoneCall.ToNumber}",
                    Metadata = $"{{\"duration\": {phoneCall.DurationSeconds}, \"city\": \"{phoneCall.CallerCity}\", \"state\": \"{phoneCall.CallerState}\"}}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeadActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lead from call");
            }
        }
    }
}
