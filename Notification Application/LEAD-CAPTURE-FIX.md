# Lead Capture Fix

## Problem
When users submitted their email in the popup preview, the lead data was not being saved to the Leads table in the database.

## Root Cause
The `handleSubmit()` function in `popup-engine.js` was only recording a conversion event for analytics, but wasn't actually sending the form data to the backend to be stored as a lead.

## Solution Implemented

### 1. Created Lead Capture API Endpoint
**File:** `/Controllers/Api/ApiControllers.cs`

Added a new endpoint `POST /api/popup/{id}/capture-lead` that:
- Accepts lead data (email, firstName, lastName, phone, company, custom fields)
- Stores the lead in the `Leads` table with full metadata (IP, user agent, UTM params)
- Records conversion analytics
- Sends data to integrations (Zapier, webhooks)
- Returns success/failure status

**Request Model:** `LeadCaptureRequest`
```csharp
public class LeadCaptureRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public Dictionary<string, string>? CustomFields { get; set; }
    public string? Referrer { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    public bool ConsentGiven { get; set; }
    public string? ConsentText { get; set; }
    public string? ViewType { get; set; }
}
```

### 2. Updated Popup Engine JavaScript
**File:** `/wwwroot/js/popup-engine.js`

Enhanced the `handleSubmit()` function to:
- Extract all form data (email, firstName, lastName, phone, etc.)
- Map common field name variations (firstName/first_name, phone/tel)
- Capture UTM parameters from URL query string
- Store any non-standard fields as custom fields
- Send lead data to `/api/popup/{id}/capture-lead` endpoint
- Handle success/error responses

**Key Features:**
- Intelligent field mapping (handles firstName, first_name, name variations)
- UTM parameter capture from URL
- Custom field support for any additional form fields
- GDPR consent tracking
- Referer and source URL tracking

## Flow After Fix

1. User fills out email form in popup
2. User clicks submit button
3. `handleSubmit()` function is triggered
4. Form data is extracted and structured
5. **NEW:** Lead data is sent to `/api/popup/{id}/capture-lead`
6. Backend creates `Lead` record in database with:
   - PopupId
   - TenantId
   - Email, FirstName, LastName, Phone, Company
   - Custom fields (JSON)
   - CapturedAt timestamp
   - IP address
   - User agent
   - Source URL (referer)
   - UTM parameters
   - GDPR consent info
   - Status (New)
7. Conversion analytics recorded
8. Integration webhooks triggered
9. Success message shown to user

## Testing

To test the lead capture:

1. Navigate to `/Popup/Preview/{popupId}` or `/Popup/LivePreview/{popupId}`
2. Fill out the email form
3. Click submit
4. Check `/Leads` page - you should see the new lead

**Console Logging:** The endpoint logs detailed information:
```
=== CAPTURE LEAD ENDPOINT HIT: Popup ID {id} ===
✓ Lead captured successfully: ID={leadId}, Email={email}
```

## Files Modified

1. `/Controllers/Api/ApiControllers.cs` - Added `CaptureLead` endpoint and `LeadCaptureRequest` model
2. `/wwwroot/js/popup-engine.js` - Enhanced `handleSubmit()` function

## Database Tables

All leads are stored in the `Leads` table with the following structure:
- Basic info: Email, FirstName, LastName, Phone, Company
- Metadata: CapturedAt, IpAddress, UserAgent, Source, Referrer
- UTM tracking: UtmSource, UtmMedium, UtmCampaign, UtmTerm, UtmContent
- GDPR: ConsentGiven, ConsentDate, ConsentText
- Management: Status (New/Contacted/Qualified/Converted/Unqualified/Archived), Notes, LastContactedAt
- Custom: CustomFields (JSON), ViewType

## Next Steps

The lead capture is now fully functional. All form submissions from popups will be:
1. Saved to the Leads table
2. Tracked in analytics
3. Sent to configured integrations (Zapier, webhooks)

You can view and manage captured leads at `/Leads` page.
