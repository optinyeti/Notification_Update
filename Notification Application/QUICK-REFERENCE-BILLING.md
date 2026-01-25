# Quick Reference: Billing & Code Separation

## New Files Created
1. `Services/BillingService.cs` - Usage tracking and limits
2. `Controllers/UserDashboardController.cs` - User operations
3. `Controllers/SuperAdminController.cs` - Platform admin
4. `wwwroot/js/feature-access.js` - Frontend feature restrictions
5. `BILLING-INTEGRATION-SUMMARY.md` - Complete documentation

## Modified Files
1. `Controllers/AdminController.cs` - Refactored to tenant-level only
2. `Program.cs` - Added BillingService to DI

## Quick Setup

### 1. Add BillingService (Already Done)
```csharp
// Program.cs
builder.Services.AddScoped<IBillingService, BillingService>();
```

### 2. Use in Designer (To be added to Designer.cshtml)
```html
<!-- Add tenant ID -->
<div class="designer-container" data-tenant-id="@User.FindFirst("TenantId")?.Value">

<!-- Include feature access script -->
<script src="~/js/feature-access.js"></script>
<script>
    window.tenantId = @User.FindFirst("TenantId")?.Value;
</script>
```

### 3. Check Feature Access (Example)
```csharp
// In any controller
public async Task<IActionResult> SomeAction()
{
    var canUse = await _billingService.CanUseAdvancedTargetingAsync(tenantId);
    if (!canUse)
    {
        TempData["Warning"] = "Upgrade required";
        return RedirectToAction("Plans", "Payment");
    }
    // ... proceed
}
```

## Route Changes

### Old → New
- `/Admin/Plans` → `/SuperAdmin/Plans`
- `/Admin/AllTenants` → `/SuperAdmin/AllTenants`
- `/Admin/Analytics` → `/UserDashboard/Analytics`
- `/Admin/Subscription` → `/UserDashboard/Subscription`
- `/Admin/Support` → `/UserDashboard/Support`

## Controller Responsibilities

| Controller | Access | Purpose |
|------------|--------|---------|
| `SuperAdminController` | SuperAdmin only | Platform management (all tenants, plans, templates) |
| `AdminController` | Admin+ | Tenant management (team, settings, blog) |
| `UserDashboardController` | All users | Personal dashboard (usage, subscription, support) |
| `PaymentController` | All users | Stripe integration (checkout, billing portal) |
| `PopupController` | All users | Popup CRUD (with usage checks) |

## API Endpoints

### Check Usage
```javascript
// Can create popup?
GET /UserDashboard/CanCreatePopup?tenantId={id}
// Response: { success: bool, message: string, remaining: int }

// Has feature?
GET /UserDashboard/CheckFeatureAccess?tenantId={id}&feature=advanced_targeting
// Response: { hasAccess: bool }

// Get usage stats
GET /UserDashboard/GetUsageStats?tenantId={id}
// Response: UsageStats object
```

### Platform Stats (SuperAdmin)
```javascript
GET /SuperAdmin/GetPlatformStats
// Returns: { totalTenants, activeTenants, totalUsers, totalPopups, ... }

GET /SuperAdmin/GetRevenueStats  
// Returns: { byPlan: {...}, totalMonthlyRevenue: decimal }
```

## Usage Limit Checks

### Backend (Automatic)
```csharp
// Already implemented in PopupService
var popup = await _popupService.CreatePopupAsync(newPopup);
// Throws InvalidOperationException if limit reached
```

### Frontend (Manual)
```javascript
// Check before creating popup
const canCreate = await window.featureAccessManager.canCreatePopup();
if (!canCreate) {
    // Modal shown automatically
    return;
}

// Check feature access
if (!window.featureAccessManager.hasAdvancedTargeting) {
    // Show upgrade prompt
}
```

## Plan Features

### Feature Flags in SubscriptionPlan
- `HasAdvancedTargeting` - Who/When/OnSite/Ecommerce categories
- `HasAnalytics` - Analytics dashboard
- `HasAPIAccess` - API endpoints
- `HasPrioritySupport` - Priority support
- `HasWhiteLabel` - White label branding

### Limits
- `MaxPopups` - Max popup count (-1 = unlimited)
- `MaxPopupViews` - Monthly views limit
- `MaxUsers` - Team member limit

## Testing

### Test Scenarios
1. **Free Plan User:**
   - Create popup → blocked at limit
   - Access advanced targeting → shown upgrade prompt
   - View analytics → redirected to plans

2. **Pro Plan User:**
   - Access all features
   - Usage bars show progress
   - Warnings when near limits

3. **SuperAdmin:**
   - Access all tenants
   - Manage plans
   - View platform stats

### Test Endpoints
```bash
# Check usage (replace tenantId)
curl http://localhost:5000/UserDashboard/GetUsageStats?tenantId=1

# Check feature
curl http://localhost:5000/UserDashboard/CheckFeatureAccess?tenantId=1&feature=advanced_targeting

# Platform stats (requires auth)
curl -H "Authorization: Bearer {token}" http://localhost:5000/SuperAdmin/GetPlatformStats
```

## Next Implementation Steps

1. **Add to Designer.cshtml:**
```html
@{
    var tenantId = User.FindFirst("TenantId")?.Value;
}

<div class="designer-container" data-tenant-id="@tenantId">
    <!-- existing content -->
</div>

@section Scripts {
    <script src="~/js/feature-access.js"></script>
    <script>
        window.tenantId = @tenantId;
    </script>
}
```

2. **Create UserDashboard Views:**
- `Views/UserDashboard/Index.cshtml` - Dashboard
- `Views/UserDashboard/Usage.cshtml` - Usage details
- `Views/UserDashboard/Subscription.cshtml` - Billing info

3. **Update Navigation:**
```html
<li><a href="/UserDashboard">Dashboard</a></li>
<li><a href="/UserDashboard/Usage">Usage</a></li>
<li><a href="/Admin">Team</a></li>
@if(User.IsInRole("SuperAdmin"))
{
    <li><a href="/SuperAdmin">Platform</a></li>
}
```

## Common Patterns

### Check and Enforce Limits
```csharp
var canCreate = await _billingService.CanCreatePopupAsync(tenantId);
if (!canCreate)
{
    TempData["Error"] = "Popup limit reached. Please upgrade.";
    return RedirectToAction("Plans", "Payment");
}
```

### Get Usage for Display
```csharp
var usage = await _billingService.GetUsageStatsAsync(tenantId);
ViewBag.PopupUsage = $"{usage.PopupsCreated}/{usage.MaxPopups ?? ∞}";
ViewBag.UsagePercent = usage.PopupUsagePercentage;
```

### Lock Feature by Plan
```javascript
// In feature-access.js (already implemented)
if (!this.hasAdvancedTargeting) {
    element.classList.add('disabled', 'requires-upgrade');
    element.onclick = () => this.showUpgradeModal('Feature Name');
}
```

## Troubleshooting

### Services not found?
Check `Program.cs` has:
```csharp
builder.Services.AddScoped<IBillingService, BillingService>();
```

### Feature access not working?
Check:
1. `feature-access.js` is included
2. `window.tenantId` is set
3. API endpoints return data
4. Plan has correct feature flags

### Routes 404?
Update links from old Admin routes to new ones:
- Check all navigation menus
- Update form actions
- Update redirects in controllers

## Support

For questions about:
- **Billing Logic:** See `BillingService.cs`
- **Feature Access:** See `feature-access.js`
- **Controller Separation:** See `BILLING-INTEGRATION-SUMMARY.md`
- **Complete Setup:** See main documentation
