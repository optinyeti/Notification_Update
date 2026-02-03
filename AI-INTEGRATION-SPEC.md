# AI Integration Specification

## Overview
Integrate AI capabilities into the Notification/Popup Management platform to provide intelligent automation for popup creation, form generation, and CRM insights.

## Features

### 1. AI Popup Generator
**Purpose**: Analyze customer website and generate targeted popup campaigns

**Workflow**:
1. User provides website URL
2. System crawls website and extracts:
   - Content themes
   - Product/service offerings
   - Target audience indicators
   - Existing CTAs
   - Design aesthetic
3. AI analyzes content and suggests 3-5 popup campaign ideas
4. User selects campaign, AI generates complete popup with:
   - Compelling headline
   - Persuasive copy
   - Appropriate popup type (email collector, discount, etc.)
   - Trigger timing recommendations
   - Targeting rules

**Technical Implementation**:
```csharp
public interface IAIPopupService
{
    Task<WebsiteAnalysis> AnalyzeWebsiteAsync(string url);
    Task<List<PopupSuggestion>> GeneratePopupSuggestionsAsync(WebsiteAnalysis analysis);
    Task<Popup> CreatePopupFromSuggestionAsync(PopupSuggestion suggestion, int tenantId);
}
```

**API Integration**:
- OpenAI GPT-4 for content generation
- Puppeteer/Playwright for website crawling
- Custom prompts optimized for marketing copy

**Example Prompt**:
```
Analyze this website content and suggest 3 popup campaign ideas:

Website: {url}
Content: {extracted_text}
Industry: {detected_industry}
Target Audience: {detected_audience}

Generate popup suggestions with:
1. Campaign goal
2. Popup type
3. Headline (max 50 chars)
4. Body copy (max 150 chars)
5. CTA text
6. Trigger timing (seconds/scroll/exit)
7. Target audience segment
```

### 2. AI Form Builder
**Purpose**: Generate custom forms based on business needs and website context

**Workflow**:
1. User describes form purpose (e.g., "contact form for B2B leads")
2. Optional: Provide website URL for context
3. AI generates:
   - Appropriate fields
   - Field validation rules
   - Form styling matching website
   - Success message
   - Email notification template

**Field Suggestions Based on Intent**:
- **Contact Form**: Name, Email, Phone, Company, Message
- **Lead Capture**: Name, Email, Industry, Company Size, Budget
- **Event Registration**: Name, Email, Company, Title, Dietary Restrictions
- **Quote Request**: Name, Email, Phone, Service Needed, Budget, Timeline

**Technical Implementation**:
```csharp
public interface IAIFormService
{
    Task<FormSuggestion> GenerateFormAsync(string intent, string websiteUrl = null);
    Task<WebsiteForm> CreateFormFromSuggestionAsync(FormSuggestion suggestion, int tenantId);
}
```

### 3. AI CRM Assistant
**Purpose**: Natural language interface to query leads, generate reports, and get insights

**Capabilities**:
- Query leads: "Show me all qualified leads from last month"
- Generate reports: "Create a report of conversion rates by source"
- Insights: "What's my best performing popup campaign?"
- Predictions: "Which leads are most likely to convert?"
- Recommendations: "How can I improve my email collection rate?"

**Technical Implementation**:
```csharp
public interface IAICRMService
{
    Task<string> QueryLeadsAsync(string naturalLanguageQuery, int tenantId);
    Task<Report> GenerateReportAsync(string reportRequest, int tenantId);
    Task<List<Insight>> GetInsightsAsync(int tenantId);
    Task<List<Recommendation>> GetRecommendationsAsync(int tenantId);
}
```

**Query Translation**:
Natural language → SQL/LINQ query → Results → Natural language summary

**Example**:
User: "Show me leads captured from Facebook last week with high scores"
→ SQL: SELECT * FROM Leads WHERE UtmSource = 'facebook' AND CapturedAt >= DATEADD(week, -1, GETDATE()) AND Score > 75
→ AI Summary: "You have 23 high-quality leads from Facebook this week, with an average score of 82..."

### 4. AI Content Optimizer
**Purpose**: Improve existing popup/form copy for better conversion

**Workflow**:
1. Select existing popup/form
2. AI analyzes current copy
3. Suggests improvements:
   - More compelling headlines
   - Stronger CTAs
   - Better value propositions
   - A/B test variations

**Metrics Considered**:
- Current conversion rate
- Industry benchmarks
- Psychological triggers
- Urgency/scarcity elements

## Architecture

```
┌─────────────────────────────────────┐
│         Frontend (Razor/JS)         │
│  - AI Popup Generator UI            │
│  - AI Form Builder UI               │
│  - AI Chat Interface                │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│       AIController.cs               │
│  - GeneratePopup()                  │
│  - GenerateForm()                   │
│  - QueryCRM()                       │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│       AI Services Layer             │
│  - AIPopupService                   │
│  - AIFormService                    │
│  - AICRMService                     │
│  - WebsiteCrawlerService            │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│    OpenAI API Integration           │
│  - GPT-4 for generation             │
│  - Embeddings for context           │
│  - Function calling                 │
└─────────────────────────────────────┘
```

## Cost Management

### Credit System
- Each tenant gets monthly AI credits based on plan:
  - Beginner: 0 credits (upgrade required)
  - Pro: 100 credits/month
  - Business: 500 credits/month
  - Enterprise: Unlimited

### Credit Usage:
- AI Popup Generation: 10 credits
- AI Form Generation: 8 credits
- CRM Query: 2 credits
- Report Generation: 15 credits
- Content Optimization: 5 credits

### Rate Limiting:
- Max 10 AI requests per hour per tenant
- Queue system for high-volume requests
- Cache common queries

## Database Schema

```sql
-- AI Credits Tracking
CREATE TABLE AICredits (
    Id INT PRIMARY KEY IDENTITY,
    TenantId INT NOT NULL,
    CreditsRemaining INT NOT NULL DEFAULT 0,
    CreditsUsedThisMonth INT NOT NULL DEFAULT 0,
    LastResetDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- AI Generation History
CREATE TABLE AIGenerations (
    Id INT PRIMARY KEY IDENTITY,
    TenantId INT NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- 'Popup', 'Form', 'Query', 'Report'
    Prompt NVARCHAR(MAX) NOT NULL,
    Response NVARCHAR(MAX),
    CreditsUsed INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL, -- 'Success', 'Failed', 'Pending'
    ErrorMessage NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessingTime INT, -- milliseconds
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- AI Model Configuration
CREATE TABLE AIModelConfig (
    Id INT PRIMARY KEY IDENTITY,
    ModelName NVARCHAR(100) NOT NULL,
    Provider NVARCHAR(50) NOT NULL, -- 'OpenAI', 'Anthropic', etc.
    ApiVersion NVARCHAR(20),
    MaxTokens INT,
    Temperature DECIMAL(3,2),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME
);
```

## Configuration

### appsettings.json
```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-...",
      "Organization": "org-...",
      "Model": "gpt-4-turbo-preview",
      "MaxTokens": 2000,
      "Temperature": 0.7
    },
    "RateLimiting": {
      "MaxRequestsPerHour": 10,
      "MaxConcurrentRequests": 3
    },
    "Credits": {
      "PopupGeneration": 10,
      "FormGeneration": 8,
      "CRMQuery": 2,
      "ReportGeneration": 15,
      "ContentOptimization": 5
    },
    "WebsiteCrawler": {
      "MaxPages": 10,
      "Timeout": 30,
      "UserAgent": "PopupManagerBot/1.0"
    }
  }
}
```

## UI Components

### AI Popup Generator Button
Location: Popup/Index page, UserDashboard
```html
<button class="btn btn-primary">
    <i class="fas fa-magic"></i> AI Generate Popup
</button>
```

### AI Chat Interface
Floating chat widget in bottom-right corner:
```html
<div id="ai-chat-widget" class="fixed bottom-4 right-4">
    <button class="ai-chat-trigger">
        <i class="fas fa-robot"></i> AI Assistant
    </button>
    <div class="ai-chat-panel hidden">
        <!-- Chat interface -->
    </div>
</div>
```

### Credit Balance Display
Show in navbar:
```html
<div class="ai-credits">
    <i class="fas fa-bolt"></i> AI Credits: 85/100
</div>
```

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Set up OpenAI API integration
- [ ] Create AI services infrastructure
- [ ] Implement credit system
- [ ] Add database tables
- [ ] Basic website crawler

### Phase 2: Popup Generation (Week 3-4)
- [ ] Website analysis engine
- [ ] Popup suggestion algorithm
- [ ] UI for AI popup generator
- [ ] Testing and refinement

### Phase 3: Form Generation (Week 5-6)
- [ ] Form intent analysis
- [ ] Field suggestion engine
- [ ] Form builder integration
- [ ] UI implementation

### Phase 4: CRM Assistant (Week 7-8)
- [ ] Natural language query parser
- [ ] Query-to-SQL translator
- [ ] Report generation
- [ ] Chat interface

### Phase 5: Optimization & Polish (Week 9-10)
- [ ] Content optimization feature
- [ ] A/B test suggestions
- [ ] Performance optimization
- [ ] User training/documentation

## Testing Strategy

### Unit Tests
- AI service methods
- Credit calculation
- Rate limiting
- Query translation

### Integration Tests
- OpenAI API calls
- Website crawling
- Database operations
- End-to-end workflows

### Load Tests
- Concurrent AI requests
- Rate limiting effectiveness
- API quota management

## Monitoring & Analytics

### Track:
- AI requests per tenant
- Success/failure rates
- Average response times
- Credit usage patterns
- Most popular AI features
- Cost per request

### Alerts:
- API quota approaching limit
- High error rates
- Slow response times
- Credit depletion

## Security Considerations

1. **API Key Protection**: Store OpenAI key in Azure Key Vault
2. **Input Sanitization**: Validate all user inputs before sending to AI
3. **Output Filtering**: Check AI responses for inappropriate content
4. **Rate Limiting**: Prevent abuse and cost overruns
5. **Audit Logging**: Track all AI usage for security review
6. **Data Privacy**: Don't send sensitive customer data to AI

## Future Enhancements

- Multi-language support
- Custom AI model fine-tuning
- Voice-based AI assistant
- Predictive lead scoring
- Automated campaign optimization
- Integration with other AI providers (Anthropic Claude, Google Gemini)
- AI-powered email writing
- Chatbot integration for websites
