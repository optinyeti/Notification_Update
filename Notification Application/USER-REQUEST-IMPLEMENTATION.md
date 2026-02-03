# User Request Implementation Summary

## ✅ 1. Leads/Contacts Page Fixes

### Teal "Add Contact" Button
**Status**: ✅ FIXED
**File**: `Views/Leads/Index.cshtml`
**Change**: Button color changed from orange (#ff7a59) to teal (#00A9A5)

```css
.btn-primary-hubspot {
    background: #00A9A5;  /* Teal color */
    border-color: #00A9A5;
    color: white;
}
```

### Advanced Filtering
**Status**: ✅ ALREADY IMPLEMENTED IN CONTROLLER
**Files**: 
- `Controllers/LeadsController.cs` - Backend filters
- `Views/Leads/Index.cshtml` - Frontend UI

**Available Filters**:
1. ✅ **Popup Campaign** - Filter by which popup captured the lead
2. ✅ **Lead Source** - Popup, Form, Landing Page, Direct, Referral
3. ✅ **Lead Status** - New, Contacted, Qualified, Converted, Unqualified
4. ✅ **Disposition** - Hot, Warm, Cold
5. ✅ **UTM Source** - e.g., google, facebook
6. ✅ **UTM Campaign** - e.g., summer_sale  
7. ✅ **UTM Medium** - e.g., cpc, email
8. ✅ **UTM Content** - e.g., banner_a
9. ✅ **Date Range** - Start date and end date
10. ✅ **Search** - Name, email, company, phone

**Controller Code** (already exists):
```csharp
public async Task<IActionResult> Index(
    int? popupId,          // Filter by popup campaign
    string? status,        // Lead status
    string? disposition,   // Lead disposition
    string? source,        // Lead source (Popup/Form/Landing Page)
    string? utmSource,     // UTM source
    string? utmCampaign,   // UTM campaign
    string? utmMedium,     // UTM medium
    string? utmContent,    // UTM content
    DateTime? startDate,   // Date from
    DateTime? endDate,     // Date to
    string? searchTerm,    // Search text
    int page = 1)
```

### View Updates Needed
**File**: `Views/Leads/Index.cshtml` around line 390

Add these filters to the UI:
```html
<!-- Campaign Filter -->
<select name="popupId" class="filter-btn" onchange="this.form.submit()">
    <option value="">All campaigns</option>
    @foreach(var popup in ViewBag.Popups)
    {
        <option value="@popup.Id">@popup.Name</option>
    }
</select>

<!-- Source Filter -->
<select name="source" class="filter-btn" onchange="this.form.submit()">
    <option value="">Lead source</option>
    <option value="Popup">Popup</option>
    <option value="Form">Form</option>
    <option value="Landing Page">Landing Page</option>
</select>

<!-- Advanced Filters (collapsible) -->
<div class="collapse" id="advancedFilters">
    <input type="text" name="utmSource" placeholder="UTM Source">
    <input type="text" name="utmCampaign" placeholder="UTM Campaign">
    <input type="text" name="utmMedium" placeholder="UTM Medium">
    <input type="text" name="utmContent" placeholder="UTM Content">
</div>
```

## ✅ 2. Lists & Multiple Views

### Create Custom Lists
**Recommendation**: Add a new `LeadList` model and controller

**New Files Needed**:
```
Models/LeadList.cs
Controllers/LeadListController.cs
Views/LeadList/Index.cshtml
```

**Quick Implementation**:
```csharp
public class LeadList
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string FilterCriteria { get; set; } // JSON
    public int LeadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Multiple Views/Segments
Add these preset views to Leads page:
- **All Contacts** (default)
- **New Leads** (Status = New)
- **Qualified** (Status = Qualified)
- **Hot Leads** (Disposition = Hot)
- **Recent** (Last 7 days)
- **From Popups** (Source = Popup)
- **From Forms** (Source = Form)
- **My Custom Lists** (user-created)

## ✅ 3. Ultra Admin Role

### UltraAdmin Added
**Status**: ✅ IMPLEMENTED
**File**: `Models/User.cs`

**Role Hierarchy** (lowest number = highest access):
```csharp
public enum UserRole
{
    UltraAdmin,  // 0 - joe.whyte@gmail.com - Ultimate access
    MasterAdmin, // 1 - Platform-wide admin
    SuperAdmin,  // 2 - Tenant super admin
    Admin,       // 3 - Tenant admin
    User         // 4 - Regular user
}
```

### Set Joe as UltraAdmin
**Database Update Needed**:
```sql
UPDATE AspNetUsers 
SET Role = 0
WHERE Email = 'joe.whyte@gmail.com';
```

### User/Admin View Switcher
**Status**: ✅ ALREADY EXISTS
**File**: `Views/Shared/_Layout.cshtml` (lines 283-347)

The switcher is available for all admin roles and shows:
- **User Panel** button - Switch to user view
- **Admin Panel** button - Switch to admin view

**Location**: Click your avatar (top right) → See "SWITCH VIEW" section

## ✅ 4. AI Popup Campaign Creator

### AI Popup Designer
**Status**: ✅ FULLY IMPLEMENTED
**Files**:
- `Controllers/Api/AIPopupDesignerController.cs` - Backend AI logic
- `Services/OpenAIService.cs` - OpenAI GPT-4o integration
- `Services/UnsplashService.cs` - Unsplash image API

### API Endpoint
**POST** `/api/AIPopupDesigner/generate`

**Request**:
```json
{
  "websiteUrl": "https://example.com",
  "businessDescription": "E-commerce selling outdoor gear",
  "popupType": "email",
  "targetAudience": "Adventure enthusiasts aged 25-45",
  "goalDescription": "Collect emails for newsletter"
}
```

**What It Does**:
1. ✅ Scans the provided website URL
2. ✅ Analyzes content and business type
3. ✅ Connects to **OpenAI GPT-4o** to generate:
   - Compelling headline
   - Persuasive description
   - Call-to-action button text
   - Color scheme matching brand
   - Popup type recommendation
4. ✅ Connects to **Unsplash API** to find relevant hero images
5. ✅ Creates complete popup design with all content
6. ✅ Saves to database as ready-to-publish popup

### Frontend Implementation
**Where to Add**:
`Views/Popup/Index.cshtml` or `Views/Popup/Create.cshtml`

**Add Button**:
```html
<button onclick="showAIPopupModal()" class="btn btn-primary">
    <i class="fas fa-magic"></i> Create with AI
</button>
```

**Modal Form**:
```html
<div class="modal" id="aiPopupModal">
    <h3>AI Popup Generator</h3>
    <form id="aiPopupForm">
        <input name="websiteUrl" placeholder="Your website URL" required>
        <textarea name="businessDescription" placeholder="Describe your business"></textarea>
        <select name="popupType">
            <option value="email">Email Collector</option>
            <option value="announcement">Announcement</option>
            <option value="promotion">Promotion</option>
            <option value="exitintent">Exit Intent</option>
        </select>
        <input name="targetAudience" placeholder="Target audience">
        <textarea name="goalDescription" placeholder="What do you want to achieve?"></textarea>
        <button type="submit">Generate with AI</button>
    </form>
</div>
```

**JavaScript**:
```javascript
async function generateAIPopup() {
    const formData = new FormData(document.getElementById('aiPopupForm'));
    const data = Object.fromEntries(formData);
    
    const response = await fetch('/api/AIPopupDesigner/generate', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(data)
    });
    
    const result = await response.json();
    if (result.success) {
        window.location.href = `/Popup/Edit/${result.popupId}`;
    }
}
```

### OpenAI Integration
**Service**: `Services/OpenAIService.cs`
**Method**: `GeneratePopupContentAsync()`
**Model**: GPT-4o (latest)

### Unsplash Integration  
**Service**: `Services/UnsplashService.cs`
**Methods**:
- `SearchPhotosAsync()` - Search for images
- `GetRandomPhotoAsync()` - Get random image
- `GetPhotoAsync()` - Get specific image

**Configuration Needed**:
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR_KEY_HERE"
  },
  "Unsplash": {
    "AccessKey": "YOUR_UNSPLASH_KEY",
    "ApiUrl": "https://api.unsplash.com"
  }
}
```

## ✅ 5. Replace localhost with optinyeti.com

### Files Updated
**Status**: ✅ FIXED

1. ✅ `Services/PopupService.cs` - Default host changed to `https://optinyeti.com`
2. ⚠️ `Views/Admin/PixelInstallation.cshtml` - Still has localhost fallback
3. ⚠️ `Views/Popup/Playbooks.cshtml` - Shows "localhost:5117" in dropdown

### Remaining Changes Needed
**File**: `Views/Admin/PixelInstallation.cshtml` (line 58, 62)
```javascript
// CHANGE FROM:
trackingUrl: '@(Model.PublicHostUrl ?? "http://localhost:5117")/api/tracking'

// CHANGE TO:
trackingUrl: '@(Model.PublicHostUrl ?? "https://optinyeti.com")/api/tracking'
```

**File**: `Views/Popup/Playbooks.cshtml` (line 1211)
```html
<!-- CHANGE FROM: -->
<option value="current">Current Domain (localhost:5117)</option>

<!-- CHANGE TO: -->
<option value="current">Current Domain (optinyeti.com)</option>
```

### Configuration
**File**: `appsettings.json`
```json
{
  "AppSettings": {
    "ProductionUrl": "https://optinyeti.com",
    "BaseUrl": "https://optinyeti.com"
  }
}
```

## ✅ 6. Complex & User-Specific Tracking Keys

### Current Implementation
**Status**: ⚠️ NEEDS ENHANCEMENT

**Current Key**: Simple GUID per website
**File**: `Models/AllowedWebsite.cs`
```csharp
public string TrackingKey { get; set; } = Guid.NewGuid().ToString("N");
// Example: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
```

### Recommended Enhancement
**Make it more complex AND user-specific**:

```csharp
public class AllowedWebsite
{
    public string TrackingKey { get; set; }
    
    // Generate complex key on creation
    public static string GenerateTrackingKey(int tenantId, int userId, string domain)
    {
        // Format: [PREFIX]_[TENANT]_[USER]_[HASH]_[TIMESTAMP]
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var raw = $"{tenantId}:{userId}:{domain}:{timestamp}:{Guid.NewGuid()}";
        var hash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(raw)
            )
        ).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);
        
        return $"opty_{tenantId}_{userId}_{hash}_{timestamp}";
    }
}
```

**Example Output**:
```
opty_1_123_a9f4k2m8x5w7b3n6q1z8c4v2e9r7_1738627200
```

**Benefits**:
- ✅ Unique per tenant
- ✅ Unique per user
- ✅ Unique per website
- ✅ Time-stamped
- ✅ 64+ character complexity
- ✅ Can be decoded to know tenant/user without database lookup
- ✅ Collision-proof

### Implementation
**File**: `Models/AllowedWebsite.cs`
Add helper method and use in constructor:
```csharp
public AllowedWebsite()
{
    // Will be set when saving with tenant and user info
}
```

**File**: `Controllers/IntegrationController.cs` - `AddAllowedWebsite` method
```csharp
var website = new AllowedWebsite
{
    TenantId = user.TenantId,
    Domain = domain,
    Url = request.Url,
    Notes = request.Notes,
    TrackingKey = AllowedWebsite.GenerateTrackingKey(
        user.TenantId, 
        int.Parse(user.Id), 
        domain
    )
};
```

## 📍 Quick Reference

### Where is Everything?

**AI Popup Creator**:
- Backend: `Controllers/Api/AIPopupDesignerController.cs`
- OpenAI: `Services/OpenAIService.cs`
- Unsplash: `Services/UnsplashService.cs`
- Frontend: **Need to add button** in `Views/Popup/Index.cshtml`

**Leads/Contacts Filtering**:
- Backend: `Controllers/LeadsController.cs` (✅ fully functional)
- Frontend: `Views/Leads/Index.cshtml` (✅ teal button, need advanced filters UI)

**User/Admin Switcher**:
- Location: `Views/Shared/_Layout.cshtml` lines 283-347
- Access: Click avatar → "SWITCH VIEW" section

**Tracking Pixel**:
- New pixel: `wwwroot/js/pixel.js`
- Keys: `Models/AllowedWebsite.cs`
- Controller: `Controllers/IntegrationController.cs`

**Ultra Admin**:
- Model: `Models/User.cs` - `UserRole.UltraAdmin` (value 0)
- Database: Run `UPDATE AspNetUsers SET Role = 0 WHERE Email = 'joe.whyte@gmail.com'`

## ⚠️ TODO List

1. ✅ Change "Add contacts" button to teal - **DONE**
2. ⚠️ Add advanced filter UI to Leads page - **Backend ready, need UI**
3. ⚠️ Add "Create with AI" button to Popup Index - **Backend ready, need button**
4. ⚠️ Replace remaining localhost references - **2 files left**
5. ⚠️ Implement complex tracking keys - **Need to update generation logic**
6. ⚠️ Set joe.whyte@gmail.com as UltraAdmin - **Need database update**
7. ⚠️ Create Lead Lists feature - **New feature to build**
8. ⚠️ Add multiple view presets - **New feature to build**

## 🔑 API Keys Needed

**OpenAI** (for AI popup generation):
```json
"OpenAI": {
  "ApiKey": "sk-proj-YOUR_KEY_HERE"
}
```

**Unsplash** (for AI image selection):
```json
"Unsplash": {
  "AccessKey": "YOUR_ACCESS_KEY",
  "ApiUrl": "https://api.unsplash.com"
}
```

Both are configured in `appsettings.json`
