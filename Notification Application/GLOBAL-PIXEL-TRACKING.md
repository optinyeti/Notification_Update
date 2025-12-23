# Global Pixel Tracking System

This document describes the event-based tracking system that uses a single global pixel instead of separate pixels for each campaign.

## Overview

The tracking system follows a **Google Tag Manager / Google Analytics 4** style architecture:

1. **Single Global Pixel** - One JavaScript file loads on the website
2. **Event-Based Tracking** - All interactions send events with metadata
3. **Campaign Attribution** - Events include `campaignId`, `variationId`, and `eventType`
4. **Server-Side Processing** - Backend handles attribution and analytics

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Website                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Global Pixel (Tenant Script)                         │  │
│  │  <script src="/Popup/GetTenantScript/1"></script>     │  │
│  └───────────────────────────────────────────────────────┘  │
│                           │                                  │
│                           ├─> Loads popup-engine.js          │
│                           ├─> Fetches campaign configs       │
│                           └─> Initializes tracking           │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Campaign 1 (Config)      Campaign 2 (Config)         │  │
│  │  - Trigger: OnPageLoad    - Trigger: OnExitIntent     │  │
│  │  - Delay: 3s              - Delay: 0s                 │  │
│  └───────────────────────────────────────────────────────┘  │
│                           │                                  │
│                    User Interactions                         │
│                           │                                  │
│              ┌────────────┼────────────┐                     │
│              │            │            │                     │
│         Impression     Click      Conversion                │
│              │            │            │                     │
└──────────────┼────────────┼────────────┼─────────────────────┘
               │            │            │
               └────────────┴────────────┘
                           │
                    Event Tracking API
                           │
          ┌────────────────┼────────────────┐
          │                │                │
     /api/tracking/event   │   /api/tracking/batch
          │                │                │
          └────────────────┴────────────────┘
                           │
                    Server Processing
                           │
          ┌────────────────┼────────────────┐
          │                │                │
     Analytics DB    Attribution    Campaign Stats
```

## Components

### 1. Global Pixel Script

**Location**: `/Popup/GetTenantScript/{tenantId}`

This is the single script that gets embedded on the website:

```html
<script src="https://yourapp.com/Popup/GetTenantScript/1"></script>
```

**What it does**:
- Loads once per page
- Prevents duplicate loading
- Fetches all published campaigns for the tenant
- Initializes popup-engine.js with tenantId
- Sets up event tracking infrastructure

**Code**: See [PopupService.cs](Services/PopupService.cs#L145-L180) `GenerateTenantScriptAsync`

### 2. Campaign Configuration

Campaigns are **not** individual scripts. They are **JSON configurations** fetched from:

**Endpoint**: `/Popup/GetTenantPopups/{tenantId}`

**Response Example**:
```json
[
  {
    "id": 1,
    "name": "Welcome Popup",
    "type": "Modal",
    "trigger": "OnPageLoad",
    "delay": 3000,
    "frequency": "OncePerSession",
    "content": "{...}",
    "showOnMobile": true,
    "showOnDesktop": true
  },
  {
    "id": 2,
    "name": "Exit Intent Offer",
    "type": "Slide",
    "trigger": "OnExitIntent",
    "delay": 0,
    "frequency": "OncePerDay",
    "content": "{...}"
  }
]
```

### 3. Event Tracking System

All events go through the **global tracking function**:

```javascript
PopupManager.track(eventType, campaignId, metadata, conversionData)
```

**Event Types**:
- `impression` - Campaign displayed to user
- `click` - User clicked CTA button
- `conversion` - User submitted form
- `close` - User closed popup

**Event Payload Example**:
```json
{
  "tenantId": 1,
  "campaignId": 5,
  "variationId": "A",
  "eventType": "impression",
  "metadata": {
    "popupName": "Welcome Popup",
    "trigger": "OnPageLoad",
    "url": "https://example.com/products",
    "referrer": "https://google.com",
    "timestamp": 1703350800000
  },
  "conversionData": null,
  "timestamp": 1703350800000
}
```

### 4. Tracking Endpoints

#### Single Event Tracking
**POST** `/api/tracking/event`

Sends individual events immediately (used for critical events like clicks/conversions).

**Request Body**:
```json
{
  "tenantId": 1,
  "campaignId": 5,
  "variationId": "A",
  "eventType": "click",
  "metadata": {
    "buttonText": "Get Started",
    "buttonType": "BUTTON"
  },
  "timestamp": 1703350800000
}
```

**Response**:
```json
{
  "success": true,
  "message": "click tracked successfully"
}
```

#### Batch Event Tracking
**POST** `/api/tracking/batch`

Sends multiple events together (automatically batched every 5 seconds).

**Request Body**:
```json
{
  "tenantId": 1,
  "events": [
    {
      "campaignId": 1,
      "eventType": "impression",
      "timestamp": 1703350800000
    },
    {
      "campaignId": 2,
      "eventType": "impression",
      "timestamp": 1703350801000
    }
  ]
}
```

**Response**:
```json
{
  "success": true,
  "processed": 2,
  "total": 2
}
```

## Implementation Details

### Client-Side (popup-engine.js)

**Key Features**:

1. **Single Initialization**:
```javascript
PopupManager.initMultiple(popupsData, tenantId)
```

2. **Automatic Event Tracking**:
```javascript
// Impression tracked automatically when popup displays
this.track('impression', popup.id, {
    popupName: popup.name,
    trigger: popup.trigger
});

// Click tracked on CTA buttons
btn.addEventListener('click', () => {
    this.track('click', popup.id, {
        buttonText: btn.textContent
    });
});

// Conversion tracked on form submission
form.addEventListener('submit', (e) => {
    const formData = {...};
    this.track('conversion', popup.id, {}, formData);
});
```

3. **Batch Processing**:
- Events queued in memory
- Sent every 5 seconds in batches
- Critical events (click/conversion) sent immediately
- Remaining events sent on page unload using `sendBeacon`

4. **Event Queue**:
```javascript
trackingQueue: []  // Holds events until batch send
```

### Server-Side (C# API)

**TrackingController** - Handles all tracking events

**Key Methods**:

1. **TrackEvent** - Process single events
```csharp
[HttpPost("event")]
public async Task<IActionResult> TrackEvent([FromBody] TrackingEventRequest request)
{
    switch (request.EventType?.ToLower())
    {
        case "impression":
            await _analyticsService.RecordPopupViewAsync(...);
            break;
        case "click":
            await _analyticsService.RecordPopupClickAsync(...);
            break;
        case "conversion":
            await _analyticsService.RecordPopupConversionAsync(...);
            break;
    }
    return Ok(new { success = true });
}
```

2. **TrackBatchEvents** - Process multiple events
```csharp
[HttpPost("batch")]
public async Task<IActionResult> TrackBatchEvents([FromBody] TrackingBatchRequest request)
{
    foreach (var evt in request.Events)
    {
        // Process each event
    }
    return Ok(new { processed = count, total = total });
}
```

**Data Models**:
```csharp
public class TrackingEventRequest
{
    public int TenantId { get; set; }
    public int CampaignId { get; set; }
    public string? VariationId { get; set; }
    public string EventType { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public Dictionary<string, object>? ConversionData { get; set; }
    public long Timestamp { get; set; }
}
```

### Database Storage

Events are stored in the `PopupAnalytics` table:

```csharp
public class PopupAnalytics
{
    public int Id { get; set; }
    public int PopupId { get; set; }  // This is the CampaignId
    public DateTime Date { get; set; }
    public int Views { get; set; }     // Impressions
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal ConversionRate { get; set; }
    
    // Device breakdown
    public int MobileViews { get; set; }
    public int DesktopViews { get; set; }
    
    // Geographic data
    public string? Country { get; set; }
    
    // Attribution data
    public string? ReferrerDomain { get; set; }
    public string? UtmSource { get; set; }
}
```

## Campaign Attribution Flow

1. **User visits website** → Global pixel loads
2. **Pixel fetches campaigns** → All active campaigns loaded as configs
3. **Campaign triggers** → Based on trigger rules (time, scroll, exit intent)
4. **Event fires** → `track('impression', campaignId, metadata)`
5. **Event queued** → Added to `trackingQueue[]`
6. **Batch sent** → Every 5 seconds to `/api/tracking/batch`
7. **Server processes** → Extracts `campaignId` from event payload
8. **Attribution stored** → Event linked to specific campaign in database
9. **Analytics calculated** → Campaign-level metrics computed

## Multiple Campaign Handling

**Question**: If 2 campaigns use the same tracking code, how does it work?

**Answer**: All campaigns share the **same global tracking code**, but each campaign:

1. **Has unique ID** - `campaignId` distinguishes campaigns
2. **Evaluated independently** - Each campaign checks its own trigger rules
3. **Tracked separately** - Events include `campaignId` for attribution
4. **Can display simultaneously** - If multiple triggers fire (though UX suggests showing one at a time)

**Example Flow**:
```
Website loads global pixel
  ↓
Fetches 3 campaigns: [Campaign 1, Campaign 2, Campaign 3]
  ↓
Campaign 1: OnPageLoad (3s delay) → Triggers → track('impression', 1, ...)
Campaign 2: OnExitIntent → Waits for exit → track('impression', 2, ...)
Campaign 3: OnScroll 50% → Waits for scroll → track('impression', 3, ...)
  ↓
All events sent with respective campaignId
  ↓
Server attributes each event to correct campaign
```

## Benefits of Global Pixel Approach

1. **Performance** ✅
   - Single script load (not N scripts)
   - Reduced network requests
   - Faster page load times

2. **Scalability** ✅
   - Add campaigns without changing website code
   - Update campaigns server-side
   - A/B testing without re-deploying

3. **Analytics** ✅
   - Unified tracking infrastructure
   - Cross-campaign attribution
   - Session-level insights

4. **Developer Experience** ✅
   - One-time installation
   - No per-campaign setup
   - Similar to GTM/GA4 workflow

## Testing

### Test Single Event
```javascript
// In browser console
PopupManager.track('click', 1, { test: true });
```

### View Tracking Queue
```javascript
console.log(PopupManager.trackingQueue);
```

### Test API Endpoint
```bash
curl -X POST https://yourapp.com/api/tracking/event \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": 1,
    "campaignId": 5,
    "eventType": "impression",
    "timestamp": 1703350800000
  }'
```

### View Analytics
- Admin Dashboard: `/Admin/Analytics`
- Per-Campaign: `/Popup/Analytics/1`

## Migration from Per-Campaign Pixels

**Old Approach** (Legacy):
```html
<!-- Campaign 1 -->
<script src="/Popup/GetPopupScript/1"></script>

<!-- Campaign 2 -->
<script src="/Popup/GetPopupScript/2"></script>

<!-- Campaign 3 -->
<script src="/Popup/GetPopupScript/3"></script>
```

**New Approach** (Recommended):
```html
<!-- Single Global Pixel -->
<script src="/Popup/GetTenantScript/1"></script>
```

**Benefits**:
- 3 HTTP requests → 1 HTTP request
- 3 script executions → 1 script execution
- Manual updates per campaign → Automatic updates
- Separate tracking → Unified tracking

## Future Enhancements

1. **Custom Events**
   - Allow tracking arbitrary events
   - Example: `track('video_watched', campaignId, {duration: 30})`

2. **A/B Testing Support**
   - `variationId` already included in events
   - Can track performance of variations
   - Server-side statistical analysis

3. **Real-Time Analytics**
   - WebSocket connection for live updates
   - Dashboard shows events as they happen

4. **User Journey Tracking**
   - Session-based attribution
   - Cross-campaign interaction paths
   - Funnel analysis

5. **Privacy Controls**
   - GDPR consent management
   - Cookie-less tracking options
   - IP anonymization

## Troubleshooting

### Events not tracking
1. Check console for errors
2. Verify `PopupManager.tenantId` is set
3. Check `PopupManager.trackingQueue` has events
4. Verify network requests to `/api/tracking/*`

### Multiple campaigns not displaying
1. Check frequency settings (OncePerSession, etc.)
2. Clear localStorage: `localStorage.clear()`
3. Check trigger conditions
4. Review console logs for "Blocked:" messages

### Analytics not updating
1. Verify events reaching server (check logs)
2. Check `PopupAnalytics` table in database
3. Ensure `_analyticsService` methods are called
4. Refresh analytics dashboard

## Summary

This global pixel tracking system provides a modern, scalable approach to campaign management and analytics. By separating the tracking infrastructure (single pixel) from campaign configuration (JSON), we achieve the flexibility of platforms like Google Tag Manager while maintaining full control over data and privacy.

**Key Takeaway**: One pixel, many campaigns, unified tracking.
