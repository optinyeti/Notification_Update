using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Notification_Application.Services;

namespace Notification_Application.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly OpenAIService _openAIService;
        private readonly ILogger<AIController> _logger;

        public AIController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            OpenAIService openAIService,
            ILogger<AIController> logger)
        {
            _context = context;
            _userManager = userManager;
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost("score-lead/{leadId}")]
        public async Task<IActionResult> ScoreLead(int leadId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

            if (lead == null)
                return NotFound(new { error = "Lead not found" });

            try
            {
                var score = await _openAIService.ScoreLeadAsync(lead);
                
                lead.Score = score;
                lead.LastScoreUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { score, message = "Lead scored successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scoring lead {LeadId}", leadId);
                return StatusCode(500, new { error = "Failed to score lead" });
            }
        }

        [HttpPost("generate-email/{leadId}")]
        public async Task<IActionResult> GenerateEmail(int leadId, [FromBody] GenerateEmailRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

            if (lead == null)
                return NotFound(new { error = "Lead not found" });

            try
            {
                var emailContent = await _openAIService.GenerateLeadEmailAsync(lead, request.EmailType);
                return Ok(new { content = emailContent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email for lead {LeadId}", leadId);
                return StatusCode(500, new { error = "Failed to generate email" });
            }
        }

        [HttpGet("lead-insights/{leadId}")]
        public async Task<IActionResult> GetLeadInsights(int leadId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

            if (lead == null)
                return NotFound(new { error = "Lead not found" });

            try
            {
                var insights = await _openAIService.GenerateLeadInsightsAsync(lead);
                return Ok(new { insights });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating insights for lead {LeadId}", leadId);
                return StatusCode(500, new { error = "Failed to generate insights" });
            }
        }

        [HttpGet("next-action/{leadId}")]
        public async Task<IActionResult> SuggestNextAction(int leadId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

            if (lead == null)
                return NotFound(new { error = "Lead not found" });

            try
            {
                var action = await _openAIService.SuggestNextActionAsync(lead);
                return Ok(new { action });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting next action for lead {LeadId}", leadId);
                return StatusCode(500, new { error = "Failed to suggest action" });
            }
        }

        [HttpPost("generate-popup-content")]
        public async Task<IActionResult> GeneratePopupContent([FromBody] GeneratePopupContentRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var content = await _openAIService.GeneratePopupContentAsync(
                    request.PopupType,
                    request.TargetAudience,
                    request.Goal);
                
                return Ok(new { content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating popup content");
                return StatusCode(500, new { error = "Failed to generate popup content" });
            }
        }

        [HttpPost("optimize-subject")]
        public async Task<IActionResult> OptimizeSubject([FromBody] OptimizeSubjectRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var optimized = await _openAIService.OptimizeEmailSubjectAsync(
                    request.OriginalSubject,
                    request.Context);
                
                return Ok(new { optimized });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing subject line");
                return StatusCode(500, new { error = "Failed to optimize subject" });
            }
        }

        [HttpPost("bulk-score-leads")]
        public async Task<IActionResult> BulkScoreLeads([FromBody] BulkScoreRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var leads = await _context.Leads
                .Where(l => request.LeadIds.Contains(l.Id) && l.TenantId == user.TenantId)
                .ToListAsync();

            if (!leads.Any())
                return NotFound(new { error = "No leads found" });

            var scoredCount = 0;
            var errors = new List<string>();

            foreach (var lead in leads)
            {
                try
                {
                    var score = await _openAIService.ScoreLeadAsync(lead);
                    lead.Score = score;
                    lead.LastScoreUpdate = DateTime.UtcNow;
                    scoredCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scoring lead {LeadId}", lead.Id);
                    errors.Add($"Failed to score lead {lead.Id}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                scored = scoredCount,
                total = leads.Count,
                errors = errors.Any() ? errors : null
            });
        }
    }

    public class GenerateEmailRequest
    {
        public string EmailType { get; set; } = "followup"; // followup, welcome, nurture
    }

    public class GeneratePopupContentRequest
    {
        public string PopupType { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public string Goal { get; set; } = "";
    }

    public class OptimizeSubjectRequest
    {
        public string OriginalSubject { get; set; } = "";
        public string Context { get; set; } = "";
    }

    public class BulkScoreRequest
    {
        public List<int> LeadIds { get; set; } = new();
    }
}
