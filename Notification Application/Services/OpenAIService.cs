using System.Text;
using System.Text.Json;
using Notification_Application.Models;

namespace Notification_Application.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OpenAIService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            
            var apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GenerateLeadEmailAsync(Lead lead, string emailType)
        {
            try
            {
                var prompt = emailType.ToLower() switch
                {
                    "followup" => $@"Write a professional follow-up email for a lead with the following information:
Name: {lead.FirstName} {lead.LastName}
Company: {lead.Company}
Email: {lead.Email}
Lead Status: {lead.Status}
Disposition: {lead.Disposition}

Write a personalized, engaging follow-up email that encourages a response. Keep it concise and professional.",

                    "welcome" => $@"Write a warm welcome email for a new lead with the following information:
Name: {lead.FirstName} {lead.LastName}
Company: {lead.Company}
Email: {lead.Email}

Write a friendly welcome email that introduces our services and builds rapport. Keep it conversational and inviting.",

                    "nurture" => $@"Write a lead nurturing email for:
Name: {lead.FirstName} {lead.LastName}
Company: {lead.Company}
Industry: {lead.Industry}

Write an educational email that provides value and keeps the lead engaged. Include helpful tips or insights relevant to their industry.",

                    _ => $@"Write a professional email for a lead:
Name: {lead.FirstName} {lead.LastName}
Company: {lead.Company}
Email: {lead.Email}

Write a clear, professional email that encourages engagement."
                };

                var response = await CallOpenAIAsync(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email for lead {LeadId}", lead.Id);
                return $"Error generating email: {ex.Message}";
            }
        }

        public async Task<int> ScoreLeadAsync(Lead lead)
        {
            try
            {
                var prompt = $@"You are a lead scoring AI. Analyze this lead and provide a score from 0-100 based on quality and conversion potential:

Lead Information:
- Name: {lead.FirstName} {lead.LastName}
- Company: {lead.Company}
- Industry: {lead.Industry}
- Job Title: {lead.JobTitle}
- Email: {lead.Email}
- Phone: {(string.IsNullOrEmpty(lead.Phone) ? "Not provided" : "Provided")}
- Lead Source: {lead.LeadSource}
- UTM Source: {lead.UtmSource}
- UTM Campaign: {lead.UtmCampaign}
- Page Views: {lead.PageViews}
- Total Visits: {lead.TotalVisits}
- Time on Site: {lead.AverageTimeOnSite} seconds
- Status: {lead.Status}
- Disposition: {lead.Disposition}

Consider:
- Data completeness
- Engagement level (page views, visits, time on site)
- Professional information quality
- Lead source quality
- Current status and disposition

Respond with ONLY a number between 0-100. No explanation, just the score.";

                var response = await CallOpenAIAsync(prompt);
                if (int.TryParse(response.Trim(), out int score))
                {
                    return Math.Clamp(score, 0, 100);
                }
                
                _logger.LogWarning("Could not parse lead score from OpenAI response: {Response}", response);
                return 50; // Default fallback score
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scoring lead {LeadId}", lead.Id);
                return 50; // Default fallback score
            }
        }

        public async Task<string> GenerateLeadInsightsAsync(Lead lead)
        {
            try
            {
                var prompt = $@"Analyze this lead and provide 3-4 actionable insights for the sales team:

Lead Profile:
- Name: {lead.FirstName} {lead.LastName}
- Company: {lead.Company}
- Industry: {lead.Industry}
- Job Title: {lead.JobTitle}
- Lead Source: {lead.LeadSource}
- UTM Campaign: {lead.UtmCampaign}
- Page Views: {lead.PageViews}
- Total Visits: {lead.TotalVisits}
- Average Time on Site: {lead.AverageTimeOnSite} seconds
- Current Status: {lead.Status}
- Disposition: {lead.Disposition}
- Notes: {(string.IsNullOrEmpty(lead.Notes) ? "None" : lead.Notes)}

Provide insights in this format:
• [Insight 1]
• [Insight 2]
• [Insight 3]

Focus on engagement patterns, conversion likelihood, and recommended next actions.";

                var response = await CallOpenAIAsync(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating insights for lead {LeadId}", lead.Id);
                return "Unable to generate insights at this time.";
            }
        }

        public async Task<string> SuggestNextActionAsync(Lead lead)
        {
            try
            {
                var prompt = $@"As a sales AI assistant, suggest the single best next action for this lead:

Lead Details:
- Name: {lead.FirstName} {lead.LastName}
- Status: {lead.Status}
- Disposition: {lead.Disposition}
- Last Contacted: {(lead.LastContactedAt.HasValue ? lead.LastContactedAt.Value.ToString("yyyy-MM-dd") : "Never")}
- Page Views: {lead.PageViews}
- Engagement: {(lead.TotalVisits > 5 ? "High" : lead.TotalVisits > 2 ? "Medium" : "Low")}

Provide a single, specific action recommendation in 1-2 sentences. Start with an action verb (e.g., 'Schedule', 'Send', 'Call', 'Follow up').";

                var response = await CallOpenAIAsync(prompt);
                return response.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting next action for lead {LeadId}", lead.Id);
                return "Follow up with a personalized email.";
            }
        }

        public async Task<string> GeneratePopupContentAsync(string popupType, string targetAudience, string goal)
        {
            try
            {
                var prompt = $@"Generate compelling popup content for:
Type: {popupType}
Target Audience: {targetAudience}
Goal: {goal}

Provide:
1. A catchy headline (max 10 words)
2. Supporting text (max 30 words)
3. Call-to-action button text (max 3 words)

Format as JSON:
{{
  ""headline"": ""..."",
  ""description"": ""..."",
  ""buttonText"": ""...""
}}";

                var response = await CallOpenAIAsync(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating popup content");
                return @"{""headline"":""Special Offer"",""description"":""Don't miss out on this limited time offer"",""buttonText"":""Get Started""}";
            }
        }

        public async Task<string> OptimizeEmailSubjectAsync(string originalSubject, string context)
        {
            try
            {
                var prompt = $@"Optimize this email subject line for higher open rates:

Original: {originalSubject}
Context: {context}

Provide 3 improved subject line alternatives that are:
- Compelling and actionable
- Under 50 characters
- Personalized when possible

Format as a numbered list.";

                var response = await CallOpenAIAsync(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing email subject");
                return originalSubject;
            }
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var apiUrl = "https://api.openai.com/v1/chat/completions";
            var model = _configuration["OpenAI:Model"] ?? "gpt-4o";
            var maxTokens = int.Parse(_configuration["OpenAI:MaxTokens"] ?? "2000");
            var temperature = double.Parse(_configuration["OpenAI:Temperature"] ?? "0.7");

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant specializing in sales and marketing." },
                    new { role = "user", content = prompt }
                },
                max_tokens = maxTokens,
                temperature = temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var messageContent = result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? string.Empty;
        }
    }
}
