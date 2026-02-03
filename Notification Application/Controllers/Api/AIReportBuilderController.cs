using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Microsoft.AspNetCore.Identity;
using Notification_Application.Models;
using OpenAI.Chat;

namespace Notification_Application.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AIReportBuilderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIReportBuilderController> _logger;

        public AIReportBuilderController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger<AIReportBuilderController> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Get relevant data for report generation
                var leads = await _context.Leads
                    .Where(l => l.TenantId == user.TenantId)
                    .OrderByDescending(l => l.CapturedAt)
                    .Take(100)
                    .ToListAsync();

                var popups = await _context.Popups
                    .Where(p => p.TenantId == user.TenantId)
                    .ToListAsync();

                var forms = await _context.WebsiteForms
                    .Where(f => f.TenantId == user.TenantId)
                    .ToListAsync();

                // Build context for AI
                var dataContext = $@"
Leads Summary:
- Total Leads: {leads.Count}
- New Leads (7 days): {leads.Count(l => l.CapturedAt >= DateTime.UtcNow.AddDays(-7))}
- Qualified Leads: {leads.Count(l => l.Status == LeadStatus.Qualified)}

Popups Summary:
- Total Popups: {popups.Count}
- Published Popups: {popups.Count(p => p.Status == PopupStatus.Published)}
- Total Views: {popups.Sum(p => p.Views)}
- Total Conversions: {popups.Sum(p => p.Conversions)}

Forms Summary:
- Total Forms: {forms.Count}
- Total Submissions: {forms.Sum(f => f.Submissions)}
";

                // Call OpenAI
                var apiKey = _configuration["OpenAI:ApiKey"];
                var client = new ChatClient("gpt-4o", apiKey);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage($@"You are an expert data analyst. Generate a comprehensive report based on the user's request and the following data:

{dataContext}

Provide actionable insights, trends, and recommendations. Format the response as clean HTML with proper headings, tables, and charts data (in JSON format for charts)."),
                    new UserChatMessage(request.ReportDescription)
                };

                var chatCompletion = await client.CompleteChatAsync(messages);
                var reportContent = chatCompletion.Value.Content[0].Text;

                // Log API usage
                await _context.ApiUsages.AddAsync(new ApiUsage
                {
                    TenantId = user.TenantId,
                    ApiType = "OpenAI",
                    Endpoint = "/api/AIReportBuilder/generate",
                    Method = "POST",
                    RequestDate = DateTime.UtcNow,
                    ResponseStatus = 200,
                    ResponseTimeMs = 0,
                    TokensUsed = chatCompletion.Value.Usage.TotalTokenCount,
                    Cost = (decimal)(chatCompletion.Value.Usage.TotalTokenCount * 0.00001),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    reportHtml = reportContent,
                    tokensUsed = chatCompletion.Value.Usage.TotalTokenCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI report");
                return StatusCode(500, new { success = false, message = "Failed to generate report" });
            }
        }
    }

    public class ReportRequest
    {
        public string ReportDescription { get; set; } = "";
    }
}
