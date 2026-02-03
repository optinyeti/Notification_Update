# Dashboard Improvements Implementation Plan

## Summary of Requirements

### 1. Dashboard Consolidation
- **Current State**: Two dashboards exist - Home/Index (root) and UserDashboard/Index (/UserDashboard)
- **Desired State**: Use UserDashboard/Index as the primary dashboard. Redirect authenticated users from Home/Index to UserDashboard/Index
- **Action**: Modify HomeController.cs to redirect authenticated users

### 2. Quick Action Cards (Top of Dashboard)
Add three prominent action buttons near the top:
- **Create Popup** → Routes to Popup/CreateCampaign
- **Create Form** → Routes to Form/Create
- **Create Number** → Routes to Numbers/Search (for Twilio number purchasing)

### 3. Remove Templates Navigation
- Remove all "Templates" links from main navigation menu
- Templates already removed in previous session
- Verify no remaining references

### 4. Remove +NewPop Button
- Search for and remove "+ New Pop" or similar button from top-level navigation (upper right)
- Based on search results, this button doesn't appear to exist currently

### 5. Usage Limits with Locked Items
Implement plan-based form limits:
- **Beginner ($7/month)**: 1 form only
- **Pro**: 5 forms
- **Next Tier**: 25 forms
- **Enterprise**: Unlimited forms

Display locked state with upgrade prompts when limit reached.

### 6. Instructional Dashboard Content
Transform dashboard into a guide that:
- Shows what the user hasn't done yet
- Suggests next steps
- Provides contextual help
- Shows progress indicators

### 7. AI Integration Planning
Create AI features:
- **AI Popup Generator**: Scan customer website and suggest/build popup campaigns
- **AI Form Builder**: Generate forms based on website analysis and intent
- **AI CRM Assistant**: Query leads, generate reports, provide insights
- **Implementation**: This requires significant backend work - OpenAI API integration, prompt engineering, etc.

### 8. Enhanced Preview Feature
- Show popup preview on actual client website (iframe or similar)
- Current preview tool + actual site context

### 9. Test URL 401 Error
- Investigate and fix 401 unauthorized error on test/preview URLs
- Check authentication middleware and API key validation

## Implementation Priority

### Phase 1 (Immediate - Today)
1. ✅ Redirect authenticated Home/Index users to UserDashboard
2. ✅ Add Quick Action Cards at top of UserDashboard
3. ✅ Verify Templates removed from navigation
4. ✅ Add form usage limits with locked states

### Phase 2 (This Week)
5. Add instructional content and progress tracking
6. Enhance dashboard with "what to do next" guidance
7. Fix 401 test URL error

### Phase 3 (Next Sprint)
8. Design AI integration architecture
9. Implement AI popup/form generation
10. Add AI CRM query assistant
11. Enhanced preview with actual website context

## Technical Implementation Details

### Form Usage Limits
```csharp
// In SubscriptionPlan model or service
public class FormLimits
{
    public static int GetMaxForms(int planId)
    {
        return planId switch
        {
            1 => 1,      // Beginner - 1 form
            2 => 1,      // If plan 2 exists
            3 => 5,      // Pro - 5 forms
            4 => 25,     // Business - 25 forms
            5 => int.MaxValue, // Enterprise - unlimited
            _ => 0
        };
    }
}
```

### AI Integration Architecture
```
User Input → AI Service → OpenAI API → Response Parser → Database/UI
               ↓
         Website Crawler
         Content Analyzer
         Template Matcher
```

### Preview Enhancement
```javascript
// Iframe approach for website preview
function showPopupOnClientSite(popupData, clientUrl) {
    const iframe = document.createElement('iframe');
    iframe.src = clientUrl;
    iframe.onload = () => {
        // Inject popup into iframe
        injectPopupCode(iframe, popupData);
    };
}
```

## Files to Modify

1. `/Controllers/HomeController.cs` - Add redirect to UserDashboard
2. `/Views/UserDashboard/Index.cshtml` - Add Quick Action cards
3. `/Controllers/FormController.cs` - Add usage limit checks
4. `/Models/SubscriptionPlan.cs` - Add form limit properties
5. `/Views/Form/Index.cshtml` - Show locked state when limit reached
6. NEW: `/Services/IAIService.cs` - AI integration interface
7. NEW: `/Services/AIService.cs` - OpenAI integration implementation

## Database Changes

```sql
-- Add AI features tracking
ALTER TABLE Tenants ADD COLUMN AICreditsRemaining INT DEFAULT 0;
ALTER TABLE Tenants ADD COLUMN AIFeaturesEnabled BIT DEFAULT 0;

-- Track AI-generated content
CREATE TABLE AIGenerations (
    Id INT PRIMARY KEY,
    TenantId INT NOT NULL,
    Type VARCHAR(50), -- 'Popup', 'Form', 'Report'
    Prompt TEXT,
    Response TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);
```

## Testing Checklist

- [ ] Authenticated users redirect to UserDashboard
- [ ] Quick action buttons work correctly
- [ ] Form limit enforced based on plan
- [ ] Locked state shows upgrade prompt
- [ ] AI service connects to OpenAI (when implemented)
- [ ] Preview shows on actual client site
- [ ] 401 error resolved for test URLs

## Notes

- AI integration is a substantial feature requiring careful planning
- Consider rate limiting and cost management for AI API calls
- Preview enhancement may require CORS configuration
- Form limits need to be enforced at create AND publish stages
