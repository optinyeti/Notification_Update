# Public Host URL Configuration

## Overview
The system now supports configurable public host URLs for embed code generation and script serving. This allows you to host the application on any domain (development, staging, production) without hardcoding localhost references.

## Configuration Hierarchy

The system resolves the public host URL in the following order:

1. **Tenant-Specific Override** (`Tenant.PublicHostUrl`)
   - Set via Admin → Settings → Public App URL
   - Highest priority - overrides all other settings
   
2. **Production URL** (`appsettings.json` → `AppSettings:ProductionUrl`)
   - Configured in appsettings.json
   - Used when no tenant override is set
   
3. **Base URL** (`appsettings.json` → `AppSettings:BaseUrl`)
   - Fallback configuration
   
4. **Request Host** (automatic detection)
   - Uses current HTTP request scheme and host
   - Default when nothing else is configured

## Setup Instructions

### 1. Configure appsettings.json

```json
{
  "AllowedHosts": "*",
  "AppSettings": {
    "BaseUrl": "http://localhost:5117",
    "ProductionUrl": "https://improved-chainsaw-g4jqrxjjg9g7cvv9g-5117.app.github.dev"
  }
}
```

### 2. Set Tenant-Specific URL (Optional)

1. Log in as admin
2. Navigate to **Admin → Settings**
3. Find "**Tenant Information**" section
4. Set "**Public App URL**" field (e.g., `https://popups.yourcompany.com`)
5. Click "**Save Changes**"

### 3. Verify Embed Code

After configuration, visit:
- **Admin → Settings** - Check "Your Tracking Code" section
- **Campaigns → Edit Campaign** - Check "Installation Code" section

The embed code should reflect your configured URL:
```html
<script src="https://your-production-url.com/Popup/GetTenantScript/3"></script>
```

## Usage

### In Admin Settings Page

The settings page shows:
- **Public App URL input**: Pre-filled with tenant override or production URL
- **Live Preview**: Updates embed code as you type
- **Copy Button**: Copies the generated code to clipboard

### In Campaign Pages

All embed code snippets (Index, Edit, Designer) automatically use the tenant's public host URL or fall back to production/base URL.

### For Multi-Tenant Setup

Each tenant can have their own public URL:
- Tenant A: `https://tenant-a.com`
- Tenant B: `https://tenant-b.com`
- Both share the same application instance but serve scripts with correct URLs

## Technical Details

### Server-Side

**PopupService.cs** methods `GeneratePopupScriptAsync` and `GenerateTenantScriptAsync`:
```csharp
var tenant = await _context.Tenants.FindAsync(tenantId);
var host = tenant?.PublicHostUrl
    ?? _configuration["AppSettings:ProductionUrl"]
    ?? _configuration["AppSettings:BaseUrl"]
    ?? "http://localhost:5117";
```

**Controllers** (PopupController, AdminController):
- Compute `ViewBag.PublicHost` for views
- Views use this for embed snippets

### Client-Side

**popup-engine.js**:
- Reads `data-api-url` attribute from script tag
- Falls back to current origin
- Sets tracking endpoints dynamically

### Database

- Migration: `20251223192032_AddPublicHostUrl`
- Column: `Tenants.PublicHostUrl` (nullable string)

## Testing

1. **Local Development**:
   - Leave `PublicHostUrl` empty
   - System uses `http://localhost:5117`

2. **Staging/Production**:
   - Set `ProductionUrl` in appsettings
   - All tenants inherit this URL unless overridden

3. **Per-Tenant URLs**:
   - Set `PublicHostUrl` for specific tenant
   - That tenant's embed codes use their custom URL

## Troubleshooting

**Problem**: Embed code still shows localhost

**Solutions**:
1. Check `appsettings.json` has `AppSettings:ProductionUrl` set
2. Verify tenant `PublicHostUrl` in Admin → Settings
3. Clear browser cache and reload the settings/campaigns page
4. Restart the application after changing appsettings.json

**Problem**: CORS errors when loading popups

**Solutions**:
1. Ensure `PublicHostUrl` matches the domain serving the application
2. If behind a reverse proxy, set the tenant override to the public-facing domain
3. Check `popup-engine.js` logs in browser console for detected base URL

## Benefits

✅ **Environment Agnostic**: Works on localhost, GitHub Codespaces, Azure, AWS, etc.  
✅ **Tenant Isolation**: Each tenant can have a custom domain  
✅ **No Hardcoding**: Eliminates localhost references in production  
✅ **Easy Migration**: Change URLs without code changes  
✅ **CORS Compliant**: Scripts load from the correct origin  
