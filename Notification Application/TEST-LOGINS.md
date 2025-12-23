# Test Login Credentials

This document contains the test accounts for verifying role-based access control.

## Test Accounts

### Admin Account
- **Email**: admin@popupmanager.com
- **Password**: Admin123!
- **Role**: Admin
- **Access**: 
  - ✅ Dashboard
  - ✅ Popup Management (create, edit, delete)
  - ✅ Analytics
  - ✅ Admin Panel (Settings, Templates, Subscriptions, Support)
  - ✅ Tenant Settings
  - ❌ SuperAdmin features (Tenant Management, User Management)

### Regular User Account
- **Email**: user@popupmanager.com
- **Password**: User123!
- **Role**: User
- **Access**: 
  - ✅ Dashboard (view only)
  - ✅ Popup Management (limited to own popups)
  - ✅ Analytics (own popups only)
  - ❌ Admin Panel (no access)
  - ❌ Tenant Settings
  - ❌ SuperAdmin features

## How Role Separation Works

### Navigation Menu
- **Regular Users**: See Dashboard, Popups, Analytics
- **Admin Users**: See Dashboard, Popups, Analytics, **Admin** menu item
- **SuperAdmin**: See all menu items plus Tenant/User management in Admin panel

### Authorization
Controllers use `[Authorize(Roles = "...")]` attributes to restrict access:

```csharp
// AdminController - Class level authorization
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    // All actions require Admin or SuperAdmin role
    
    // Some actions restricted to SuperAdmin only
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Tenants() { ... }
}
```

### After Login
- Both user types redirect to dashboard (`Home/Index`)
- Dashboard shows different content based on role
- Navigation menu automatically hides/shows items based on role
- Attempting to access unauthorized pages returns 403 Forbidden

## Testing Steps

1. **Test Admin Login**:
   - Go to `/Account/Login`
   - Enter: admin@popupmanager.com / Admin123!
   - Should see "Admin" menu item in navigation
   - Can access `/Admin/Index`

2. **Test Regular User Login**:
   - Logout if needed
   - Go to `/Account/Login`
   - Enter: user@popupmanager.com / User123!
   - Should NOT see "Admin" menu item
   - Attempting to access `/Admin/Index` returns 403 Forbidden

3. **Test Authorization**:
   - While logged in as regular user, try accessing:
     - `/Admin/Index` → 403 Forbidden
     - `/Admin/Settings` → 403 Forbidden
     - `/Popup/Index` → ✅ Allowed
     - `/Home/Index` → ✅ Allowed

## Database Seeding

The test accounts are automatically created when the application starts via `DatabaseSeeder.cs`:

```csharp
// Creates admin account
await userManager.CreateAsync(adminUser, "Admin123!");
await userManager.AddToRoleAsync(adminUser, "Admin");

// Creates regular user account
await userManager.CreateAsync(regularUser, "User123!");
await userManager.AddToRoleAsync(regularUser, "User");
```

## Role Hierarchy

1. **SuperAdmin** (highest)
   - Full system access
   - Can manage all tenants
   - Can manage all users
   - Access to all features

2. **Admin** (middle)
   - Full access within own tenant
   - Can manage tenant settings
   - Can manage tenant popups
   - Can view tenant analytics
   - Cannot manage other tenants

3. **User** (lowest)
   - Limited access within own tenant
   - Can create/edit own popups
   - Can view own analytics
   - Cannot access admin features
   - Cannot manage tenant settings

## Notes

- All accounts belong to the "Default Company" tenant
- Both accounts share the same tenant data (popups, analytics)
- Role separation is enforced at the controller/action level
- ASP.NET Core Identity handles authentication
- Role-based authorization is declarative via `[Authorize]` attributes
