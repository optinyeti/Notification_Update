using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Notification_Application.Services;
using System.Security.Claims;
using System.Text.Json;

namespace Notification_Application.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AIPopupDesignerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly OpenAIService _openAIService;
    private readonly ILogger<AIPopupDesignerController> _logger;

    public AIPopupDesignerController(
        ApplicationDbContext context,
        OpenAIService openAIService,
        ILogger<AIPopupDesignerController> logger)
    {
        _context = context;
        _openAIService = openAIService;
        _logger = logger;
    }

    [HttpPost("generate-popup")]
    public async Task<IActionResult> GenerateCompletePopup([FromBody] GeneratePopupRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Tenant)
                .ThenInclude(t => t.SubscriptionPlan)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized();

            // Check AI access
            if (user.Tenant.SubscriptionPlan.MaxAIRequests == 0)
            {
                return BadRequest(new { error = "Your plan does not include AI features. Upgrade to Professional or Enterprise." });
            }

            // Check AI limits
            if (user.Tenant.SubscriptionPlan.MaxAIRequests > 0)
            {
                // TODO: Implement usage tracking
                // For now, just check the plan has access
            }

            // Generate popup design using AI
            var popupDesign = await GeneratePopupDesignAsync(request);

            // Create the popup in database
            var popup = new Popup
            {
                Name = popupDesign.Name,
                Type = popupDesign.Type,
                Title = popupDesign.Title,
                Subtitle = popupDesign.Subtitle,
                CallToAction = popupDesign.CallToAction,
                Content = popupDesign.Content,
                Trigger = popupDesign.Trigger,
                DelayMs = popupDesign.DelayMs,
                Frequency = popupDesign.Frequency,
                ShowOnMobile = true,
                ShowOnDesktop = true,
                Status = PopupStatus.Draft,
                TenantId = user.TenantId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Popups.Add(popup);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                popupId = popup.Id,
                design = popupDesign,
                message = "AI-generated popup created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating popup with AI");
            return StatusCode(500, new { error = "Failed to generate popup" });
        }
    }

    [HttpPost("suggest-improvements")]
    public async Task<IActionResult> SuggestPopupImprovements([FromBody] ImprovementRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var popup = await _context.Popups
                .Include(p => p.Tenant)
                .ThenInclude(t => t.SubscriptionPlan)
                .FirstOrDefaultAsync(p => p.Id == request.PopupId && p.CreatedById == userId);

            if (popup == null)
                return NotFound();

            if (popup.Tenant.SubscriptionPlan.MaxAIRequests == 0)
            {
                return BadRequest(new { error = "AI features require Professional or Enterprise plan" });
            }

            var improvements = await SuggestImprovementsAsync(popup);

            return Ok(new { improvements });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting improvements");
            return StatusCode(500, new { error = "Failed to generate suggestions" });
        }
    }

    private async Task<PopupDesign> GeneratePopupDesignAsync(GeneratePopupRequest request)
    {
        var prompt = $@"You are an expert popup designer. Create a complete popup design based on these requirements:

**Goal**: {request.Goal}
**Industry**: {request.Industry}
**Target Audience**: {request.TargetAudience}
**Popup Type**: {request.PopupType}
**Tone**: {request.Tone ?? "Professional and friendly"}

Generate a complete popup design with:
1. A compelling headline (max 60 characters)
2. Supporting text/description (max 150 characters)
3. Call-to-action button text (max 25 characters)
4. Recommended colors (primary and accent)
5. Trigger type (OnPageLoad, OnExitIntent, OnScroll, OnTimeDelay)
6. Delay timing in milliseconds
7. Frequency (EveryVisit, OncePerSession, OncePerDay, OncePerWeek)

The popup should be:
- Attention-grabbing but not annoying
- Clear and concise
- Action-oriented
- Mobile-friendly
- Conversion-optimized

Provide the response as a JSON object with this exact structure:
{{
  ""name"": ""Popup Name"",
  ""title"": ""Headline Text"",
  ""subtitle"": ""Supporting description"",
  ""callToAction"": ""CTA Button Text"",
  ""primaryColor"": ""#HEX"",
  ""accentColor"": ""#HEX"",
  ""trigger"": ""OnPageLoad|OnExitIntent|OnScroll|OnTimeDelay"",
  ""delayMs"": 0,
  ""frequency"": ""OncePerSession|OncePerDay|EveryVisit"",
  ""explanation"": ""Brief explanation of design choices""
}}";

        var apiUrl = "https://api.openai.com/v1/chat/completions";
        var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = "You are an expert conversion rate optimization and popup design specialist. Always respond with valid JSON only." },
                new { role = "user", content = prompt }
            },
            response_format = new { type = "json_object" },
            temperature = 0.8,
            max_tokens = 1000
        };

        var response = await client.PostAsJsonAsync(apiUrl, requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var content = result?.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        var aiResponse = JsonSerializer.Deserialize<AIPopupResponse>(content!);

        // Map AI response to popup design
        var design = new PopupDesign
        {
            Name = aiResponse.name,
            Title = aiResponse.title,
            Subtitle = aiResponse.subtitle,
            CallToAction = aiResponse.callToAction,
            Type = ParsePopupType(request.PopupType),
            Trigger = ParseTrigger(aiResponse.trigger),
            DelayMs = aiResponse.delayMs,
            Frequency = ParseFrequency(aiResponse.frequency),
            Content = GeneratePopupContent(aiResponse, request.PopupType)
        };

        return design;
    }

    private async Task<List<string>> SuggestImprovementsAsync(Popup popup)
    {
        var prompt = $@"Analyze this popup and suggest 5 specific improvements:

**Current Popup**:
- Title: {popup.Title}
- Subtitle: {popup.Subtitle}
- CTA: {popup.CallToAction}
- Type: {popup.Type}
- Trigger: {popup.Trigger}
- Performance: {popup.Views} views, {popup.Clicks} clicks, {(popup.Views > 0 ? (popup.Clicks * 100.0 / popup.Views).ToString("F1") : "0")}% CTR

Suggest improvements for:
1. Headline clarity and impact
2. Description persuasiveness
3. CTA effectiveness
4. Timing and trigger optimization
5. Visual design and layout

Be specific and actionable. Format as a JSON array of improvement strings.";

        var apiUrl = "https://api.openai.com/v1/chat/completions";
        var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = "You are a conversion optimization expert. Provide actionable suggestions." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        var response = await client.PostAsJsonAsync(apiUrl, requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var content = result?.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        var improvements = JsonSerializer.Deserialize<List<string>>(content!);
        return improvements ?? new List<string>();
    }

    private string GeneratePopupContent(AIPopupResponse aiResponse, string popupType)
    {
        // Generate HTML/JSON content based on popup type
        var content = new
        {
            version = "1.0",
            type = popupType,
            design = new
            {
                colors = new
                {
                    primary = aiResponse.primaryColor,
                    accent = aiResponse.accentColor,
                    text = "#333333",
                    background = "#FFFFFF"
                },
                fonts = new
                {
                    heading = "Inter, sans-serif",
                    body = "Inter, sans-serif"
                },
                layout = new
                {
                    width = "450px",
                    padding = "40px",
                    borderRadius = "12px",
                    boxShadow = "0 20px 60px rgba(0,0,0,0.3)"
                }
            },
            content = new
            {
                headline = aiResponse.title,
                description = aiResponse.subtitle,
                button = new
                {
                    text = aiResponse.callToAction,
                    style = "primary"
                }
            },
            aiGenerated = true,
            aiExplanation = aiResponse.explanation
        };

        return JsonSerializer.Serialize(content);
    }

    private PopupType ParsePopupType(string type)
    {
        return type.ToLower() switch
        {
            "email" or "emailcollector" => PopupType.EmailCollector,
            "message" or "announcement" => PopupType.Message,
            "advertising" or "promo" => PopupType.Advertising,
            "lightbox" => PopupType.Lightbox,
            "inline" => PopupType.Inline,
            "spinwheel" or "gamification" => PopupType.SpinWheel,
            "video" => PopupType.VideoPopup,
            "coupon" or "discount" => PopupType.Coupon,
            "promotionbar" or "bar" => PopupType.PromotionBar,
            _ => PopupType.EmailCollector
        };
    }

    private PopupTrigger ParseTrigger(string trigger)
    {
        return trigger switch
        {
            "OnPageLoad" => PopupTrigger.OnPageLoad,
            "OnExitIntent" => PopupTrigger.OnExitIntent,
            "OnScroll" => PopupTrigger.OnScroll,
            "OnTimeDelay" => PopupTrigger.OnTimeDelay,
            "OnClick" => PopupTrigger.OnClick,
            "OnIdle" => PopupTrigger.OnIdle,
            _ => PopupTrigger.OnPageLoad
        };
    }

    private PopupFrequency ParseFrequency(string frequency)
    {
        return frequency switch
        {
            "EveryVisit" => PopupFrequency.EveryVisit,
            "OncePerSession" => PopupFrequency.OncePerSession,
            "OncePerDay" => PopupFrequency.OncePerDay,
            "OncePerWeek" => PopupFrequency.OncePerWeek,
            "OncePerMonth" => PopupFrequency.OncePerMonth,
            "OnceEver" => PopupFrequency.OnceEver,
            _ => PopupFrequency.OncePerSession
        };
    }
}

// Request/Response Models
public class GeneratePopupRequest
{
    public string Goal { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string PopupType { get; set; } = "EmailCollector";
    public string? Tone { get; set; }
}

public class ImprovementRequest
{
    public int PopupId { get; set; }
}

public class AIPopupResponse
{
    public string name { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string subtitle { get; set; } = string.Empty;
    public string callToAction { get; set; } = string.Empty;
    public string primaryColor { get; set; } = "#6366f1";
    public string accentColor { get; set; } = "#8b5cf6";
    public string trigger { get; set; } = "OnPageLoad";
    public int delayMs { get; set; } = 0;
    public string frequency { get; set; } = "OncePerSession";
    public string explanation { get; set; } = string.Empty;
}

public class PopupDesign
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string CallToAction { get; set; } = string.Empty;
    public PopupType Type { get; set; }
    public PopupTrigger Trigger { get; set; }
    public int DelayMs { get; set; }
    public PopupFrequency Frequency { get; set; }
    public string Content { get; set; } = string.Empty;
}
