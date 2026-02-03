# Pixel Tracking System - Complete Implementation

## Overview
Fully modernized tracking pixel system that controls popups, forms, landing pages, and CRM functionality with per-website unique tracking capability.

## Changes Made

### 1. Created Universal Tracking Pixel (`wwwroot/js/pixel.js`)
**File**: `/wwwroot/js/pixel.js`

**Features**:
- **Universal Tracking**: Single pixel handles popups, forms, landing pages, and CRM
- **Automatic Detection**: Extracts tenant key from script URL parameter
- **Event Tracking**: Pageviews, popup interactions, form submissions, lead identification
- **Batch Processing**: Queues events and sends in batches every 10 seconds
- **Lead Persistence**: Stores lead email in localStorage for session tracking
- **Device Detection**: Automatically detects device type and browser
- **Auto-loading**: Automatically loads popups and forms for the tenant

**Key Functions**:
```javascript
PixelTracker.track(eventType, data)      // Track any event
PixelTracker.identify(email, data)      // Identify a lead
PixelTracker.flush()                     // Send queued events immediately
PixelTracker.loadPopups()                // Load tenant's popups
PixelTracker.loadForms()                 // Setup form tracking
```

### 2. Updated Integration Page
**File**: `Views/Integration/Index.cshtml`

**Improvements**:
- Shows actual app URL and tenant key (no more placeholders)
- Clear explanation that one pixel tracks everything
- "What It Tracks" expandable section showing all capabilities
- Per-website pixel scripts with unique tracking keys
- Modern UI with visual enhancements

**Before**:
```html
<script src="https://yourapp.com/pixel.js?key=YOUR_TENANT_KEY" async></script>
```

**After**:
```html
<script src="http://localhost:5117/js/pixel.js?key=abc123xyz789" async></script>
```

### 3. Enhanced IntegrationController
**File**: `Controllers/IntegrationController.cs`

**Added ViewBag Properties**:
```csharp
ViewBag.TenantKey = tenant?.TrackingCode ?? tenant?.ApiKey ?? user.TenantId.ToString();
ViewBag.AppUrl = tenant?.PublicHostUrl ?? $"{Request.Scheme}://{Request.Host}";
```

Now passes real values to the view instead of placeholders.

### 4. Updated AllowedWebsite Model
**File**: `Models/AllowedWebsite.cs`

**Added Property**:
```csharp
public string TrackingKey { get; set; } = Guid.NewGuid().ToString("N");
```

Each website gets its own unique tracking key for isolated analytics.

### 5. Enhanced PixelController API
**File**: `Controllers/Api/PixelController.cs`

**Updated Both Endpoints** (`Track` and `IdentifyLead`):
- Now accepts tenant-wide keys OR website-specific tracking keys
- Automatically looks up website by TrackingKey if not found by tenant key
- Seamless support for multi-site tracking

**Code**:
```csharp
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
```

### 6. Enhanced Website List Display
**File**: `Views/Integration/Index.cshtml`

**Added**:
- Per-website pixel script display
- Copy button for each website's unique pixel
- Visual separation showing website-specific tracking
- Informative message explaining unique pixel benefits

## Database Migration

**Migration**: `20260203033242_AddWebsiteTrackingKey`

**Changes**:
- Added `TrackingKey` column to `AllowedWebsites` table
- Default value: Unique GUID for each website
- Nullable: No (always has a value)

## How It Works

### For Single Website Users:
1. Copy the main tracking pixel from Integration page
2. Install on website (paste in `<head>` or via GTM)
3. Pixel tracks everything: popups, forms, pages, leads
4. All data attributed to tenant

### For Multi-Website Users:
1. Add each website in "Allowed Websites" section
2. Each website gets a unique pixel script
3. Copy website-specific pixel for each domain
4. Separate tracking per website while all data goes to same tenant
5. Can segment analytics by website using TrackingKey

### Pixel Functionality:
1. **Pageviews**: Automatic on page load
2. **Popups**: Auto-loads and tracks views, clicks, conversions
3. **Forms**: Tracks views, submissions, abandonment
4. **Leads**: Identifies visitors when email captured
5. **CRM**: Creates lead activities, updates scores
6. **Heartbeat**: Tracks time on page every 30 seconds

## API Endpoints

### POST /api/Pixel/Track
Tracks any event (pageview, popup_view, form_submit, etc.)

**Request**:
```json
{
  "tenantKey": "abc123xyz789",
  "eventType": "popup_view",
  "email": "user@example.com",
  "pageUrl": "https://example.com/page",
  "pageTitle": "Product Page",
  "timeOnPage": 45,
  "deviceType": "desktop",
  "browser": "Chrome",
  "eventData": "{\"popupId\":123}"
}
```

### POST /api/Pixel/IdentifyLead
Identifies a lead when email is captured

**Request**:
```json
{
  "tenantKey": "abc123xyz789",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1234567890",
  "company": "Acme Corp",
  "pageUrl": "https://example.com/signup",
  "source": "Website Form"
}
```

## Installation Instructions

### Direct Install:
```html
<!-- Paste in <head> tag of your website -->
<script src="https://yourapp.com/js/pixel.js?key=YOUR_TENANT_KEY" async></script>
```

### Google Tag Manager:
1. Log in to Google Tag Manager
2. Create new tag: **Custom HTML**
3. Paste the pixel script above
4. Set trigger to: **All Pages**
5. Save and publish container

## Features Summary

✅ **One Pixel Does Everything**
- Popups (views, clicks, conversions)
- Forms (views, submissions, abandonment)
- Landing pages (visits, time, scrolling)
- CRM (lead tracking, activities, scoring)

✅ **Per-Website Tracking**
- Unique pixel per website
- Isolated analytics
- All data in one tenant account

✅ **Automatic Features**
- Lead identification when email captured
- Session tracking across pages
- Device and browser detection
- Batch event processing
- localStorage persistence

✅ **Modern Implementation**
- No placeholders - real URLs and keys
- Clear documentation
- Visual UI enhancements
- Copy buttons for easy installation

## Files Changed

1. **Created**: `wwwroot/js/pixel.js` (new universal tracking pixel)
2. **Modified**: `Views/Integration/Index.cshtml` (enhanced UI, per-site pixels)
3. **Modified**: `Controllers/IntegrationController.cs` (pass real values)
4. **Modified**: `Models/AllowedWebsite.cs` (added TrackingKey)
5. **Modified**: `Controllers/Api/PixelController.cs` (support website keys)
6. **Created**: `Migrations/20260203033242_AddWebsiteTrackingKey.cs` (database)

## Testing

### Test Installation:
1. Go to Integration page
2. Copy pixel script (has real values)
3. Test on any website
4. Check browser console for: `[Pixel] Tracking active ✓`

### Test Tracking:
```javascript
// In browser console
PixelTracker.track('test_event', { test: true });
PixelTracker.identify('test@example.com', { firstName: 'Test' });
```

### Verify in Database:
```sql
SELECT * FROM LeadActivities ORDER BY CreatedAt DESC LIMIT 10;
SELECT * FROM Leads ORDER BY CapturedAt DESC LIMIT 10;
```

## Next Steps

1. ✅ Pixel tracking restored and modernized
2. ✅ Per-website unique tracking implemented
3. ✅ No more placeholder text
4. ⚠️ Test pixel with live popups
5. ⚠️ Test form tracking
6. ⚠️ Test lead identification
7. ⚠️ Monitor API usage and performance

## Support

**Tracked Events**:
- `pageview` - Page visits
- `popup_view` - Popup impressions
- `popup_click` - Button clicks in popups
- `popup_close` - Popup dismissals
- `form_view` - Form impressions
- `form_submit` - Form submissions
- `lead_identified` - Email captured
- `heartbeat` - Time tracking

All events automatically create lead activities and update CRM data.
