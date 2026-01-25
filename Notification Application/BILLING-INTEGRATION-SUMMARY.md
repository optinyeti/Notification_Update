# Billing Integration & Code Separation Summary

## Overview
This document outlines the major refactoring performed to separate admin and user functionality, and integrate billing/usage limits throughout the application.

## Changes Made

### 1. **New Services Created**

#### BillingService (`Services/BillingService.cs`)
- Centralized service for managing billing and usage limits
- **Key Methods:**
  - `GetUsageStatsAsync()` - Returns current usage statistics (popups, views, team members)
  - `CanCreatePopupAsync()` - Checks if tenant can create more popups
  - `CanUseAdvancedTargetingAsync()` - Checks if plan includes advanced targeting
  - `CanUseAnalyticsAsync()` - Checks if plan includes analytics
  - `CanUseApiAsync()` - Checks if plan includes API access
  - `GetBillingInfoAsync()` - Returns complete billing information
  - `GetFeatureUsageAsync()` - Returns usage dictionary for dashboard
  - `ResetMonthlyUsageAsync()` - Resets monthly counters

- **Models:**
  - `UsageStats` - Complete usage statistics with percentages
  - `BillingInfo` - Billing and subscription information
  - `BillingEvent` - Transaction history events

### 2. **New Controllers**

#### UserDashboardController (`Controllers/UserDashboardController.cs`)
**Purpose:** User-specific operations (all users including Admin/SuperAdmin)

**Routes:**
- `/UserDashboard/Index` - Main dashboard with usage stats
- `/UserDashboard/Subscription` - View subscription and billing info
- `/UserDashboard/Usage` - Detailed usage statistics
- `/UserDashboard/Popups` - List of popups
- `/UserDashboard/Analytics` - Analytics dashboard (requires plan feature)
- `/UserDashboard/Support` - Support tickets

**API Endpoints:**
- `/UserDashboard/CanCreatePopup` - Check popup creation limits
- `/UserDashboard/CheckFeatureAccess` - Check feature availability
- `/UserDashboard/GetUsageStats` - Get current usage data
- `/UserDashboard/PopupCountSummary` - Popup count summary
- `/UserDashboard/AnalyticsSummary` - Analytics summary

#### SuperAdminController (`Controllers/SuperAdminController.cs`)
**Purpose:** Platform-level administration (SuperAdmin only)

**Routes:**
- `/SuperAdmin/Index` - Platform dashboard
- `/SuperAdmin/AllTenants` - View all tenants
- `/SuperAdmin/TenantDetails/{id}` - View specific tenant
- `/SuperAdmin/Plans` - Manage subscription plans
- `/SuperAdmin/CreatePlan` - Create new plan
- `/SuperAdmin/EditPlan/{id}` - Edit subscription plan
- `/SuperAdmin/Templates` - Manage popup templates
- `/SuperAdmin/Analytics` - Platform-wide analytics

**API Endpoints:**
- `/SuperAdmin/GetPlatformStats` - Platform statistics
- `/SuperAdmin/GetRevenueStats` - Revenue by plan
- `/SuperAdmin/SyncPopupCount` - Sync popup counts
- `/SuperAdmin/ResetPopupCount` - Reset popup counts

### 3. **Refactored Controllers**

#### AdminController (`Controllers/AdminController.cs`)
**Purpose:** Tenant-level administration (Admin/SuperAdmin for their tenant)

**Removed:**
- All platform-level operations (moved to SuperAdminController)
- All user dashboard operations (moved to UserDashboardController)
- Support ticket operations (moved to UserDashboardController)
- Subscription view (moved to UserDashboardController)

**Kept:**
- `/Admin/Index` - Tenant admin dashboard
- `/Admin/Users` - Team member management
- `/Admin/EditUser` - Edit team member
- `/Admin/Settings` - Tenant settings
- `/Admin/Blog` - Blog management
- `/Admin/BlogCategories` - Blog category management

**New Dependencies:**
- Added `IBillingService` for usage tracking
- Removed `IAnalyticsService`, `ISupportService`, `IPopupTemplateService` (not needed for tenant admin)

### 4. **Frontend Integration**

#### feature-access.js (`wwwroot/js/feature-access.js`)
JavaScript module for enforcing feature access on the frontend

**Features:**
- Checks if user has advanced targeting access
- Disables restricted Display Rules categories
- Shows upgrade prompts and modals
- Displays usage statistics in UI
- Warns when approaching limits
- Prevents popup creation when limit reached

**Restricted Categories:**
- `who` - Who/Personalization
- `when` - When/Triggers (advanced)
- `onsite` - OnSite Retargeting  
- `ecommerce` - Ecommerce tracking

**Usage:**
```html
<script src="~/js/feature-access.js"></script>
<script>
    window.tenantId = @Model.TenantId;
</script>
```

### 5. **Database Integration**

#### Existing Models (No changes required)
- `SubscriptionPlan` - Already has feature flags
  - `HasAdvancedTargeting`
  - `HasAnalytics`
  - `HasAPIAccess`
  - `HasPrioritySupport`
  - `HasWhiteLabel`
  - `MaxPopups` - Popup limit (-1 = unlimited)
  - `MaxPopupViews` - Monthly view limit
  - `MaxUsers` - Team member limit

- `Tenant` - Already tracks usage
  - `PopupCount` - Current popup count
  - `MonthlyPopupViews` - Monthly views
  - `LastUsageReset` - Last monthly reset
  - `SubscriptionStatus` - Stripe subscription status

## Code Separation Strategy

### Before (Intertwined)
```
AdminController
├── Platform operations (all tenants, plans, templates)
├── Tenant operations (settings, team)
├── User operations (dashboard, analytics)
└── Support operations (tickets)
```

### After (Separated)
```
SuperAdminController (Platform-level)
├── All tenants management
├── Subscription plans management
├── Popup templates management
└── Platform analytics

AdminController (Tenant-level)
├── Tenant settings
├── Team member management
└── Blog management

UserDashboardController (User-level)
├── Personal dashboard
├── Usage statistics
├── Subscription management
├── Support tickets
└── Analytics (if plan allows)
```

## Usage Limit Enforcement

### Backend (Automatic)
1. **Popup Creation** - `PopupService.CreatePopupAsync()`
   - Calls `_tenantService.CheckUsageLimitsAsync(tenantId, "popups")`
   - Throws exception if limit reached

2. **Monthly Views** - `AnalyticsService.TrackViewAsync()`
   - Increments `tenant.MonthlyPopupViews`
   - Resets monthly if needed

3. **Feature Access**
   - Controllers check `plan.HasAdvancedTargeting`, `plan.HasAnalytics`, etc.
   - Return unauthorized/redirect if feature not available

### Frontend (User Experience)
1. **Display Rules** - `feature-access.js`
   - Disables advanced categories
   - Shows upgrade prompts
   - Prevents rule creation

2. **Popup Creation**
   - Checks limit before showing designer
   - Shows upgrade modal if limit reached

3. **Dashboard**
   - Displays usage bars
   - Shows upgrade CTAs
   - Warns when near limits

## Integration Points

### To Connect Display Rules to Billing:
1. Add tenant ID to Designer.cshtml:
```html
<div class="designer-container" data-tenant-id="@Model.TenantId">
```

2. Include feature-access.js:
```html
<script src="~/js/feature-access.js"></script>
```

3. Check advanced targeting in condition builder:
```javascript
if (!window.featureAccessManager.hasAdvancedTargeting) {
    // Show upgrade prompt
}
```

### To Add Usage Dashboard:
1. Use UserDashboardController routes
2. Display UsageStats model
3. Show progress bars for limits
4. Add upgrade CTAs

## API Endpoints Reference

### Billing & Usage
- `GET /UserDashboard/GetUsageStats?tenantId={id}` - Current usage
- `GET /UserDashboard/CanCreatePopup?tenantId={id}` - Check popup limit
- `GET /UserDashboard/CheckFeatureAccess?tenantId={id}&feature={name}` - Check feature

### Platform Stats (SuperAdmin only)
- `GET /SuperAdmin/GetPlatformStats` - Platform overview
- `GET /SuperAdmin/GetRevenueStats` - Revenue breakdown

## Next Steps

### Recommended Enhancements:
1. **Create Usage Dashboard Views** - Add Razor views for UserDashboardController
2. **Add Usage Widgets** - Create reusable components for usage bars
3. **Email Notifications** - Alert users when approaching limits
4. **Billing History** - Implement transaction logging
5. **Usage Reports** - Add exportable usage reports
6. **Trial Management** - Add trial period handling
7. **Upgrade Flows** - Streamline upgrade process
8. **Downgrade Protection** - Handle downgrades gracefully

### Testing Checklist:
- [ ] Free plan users cannot access advanced targeting
- [ ] Popup creation blocked at limit
- [ ] Usage stats display correctly
- [ ] Monthly views reset properly
- [ ] SuperAdmin can manage all tenants
- [ ] Tenant admin can only manage their team
- [ ] Regular users can view their usage
- [ ] Upgrade prompts appear for locked features
- [ ] Stripe webhooks update subscription status

## File Structure Summary

```
Controllers/
├── AdminController.cs (Tenant-level admin)
├── SuperAdminController.cs (Platform admin)
├── UserDashboardController.cs (User operations)
├── PaymentController.cs (Stripe integration)
└── PopupController.cs (Popup CRUD)

Services/
├── BillingService.cs (NEW - Usage tracking)
├── TenantService.cs (Tenant operations)
├── PopupService.cs (Uses billing checks)
└── IServices.cs (Interface definitions)

wwwroot/js/
└── feature-access.js (NEW - Frontend restrictions)

Models/
├── SubscriptionPlan.cs (Plan definitions)
├── Tenant.cs (Usage tracking)
└── User.cs (User roles)
```

## Breaking Changes

### For Existing Code:
1. **AdminController routes changed:**
   - `/Admin/Plans` → `/SuperAdmin/Plans`
   - `/Admin/AllTenants` → `/SuperAdmin/AllTenants`
   - `/Admin/Analytics` → `/UserDashboard/Analytics`
   - `/Admin/Subscription` → `/UserDashboard/Subscription`

2. **New dependency injection required:**
```csharp
// Add to Program.cs
builder.Services.AddScoped<IBillingService, BillingService>();
```

3. **Views need updating:**
   - Admin dashboard views need new ViewModel
   - Create new views for UserDashboard
   - Create new views for SuperAdmin

## Migration Guide

### For Developers:
1. Update all links to admin routes in navigation
2. Add IBillingService to controllers that need usage checks
3. Include feature-access.js in pages with restricted features
4. Update authorization attributes on routes
5. Test all role-based access controls

### For Database:
No migration required - existing schema supports all features.

## Conclusion

The application is now properly separated into three distinct layers:
1. **SuperAdmin** - Platform management
2. **Admin** - Tenant management  
3. **User** - Personal operations

Billing and usage limits are enforced at every level, with clear upgrade paths for users who reach their limits.
