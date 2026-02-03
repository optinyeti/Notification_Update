# OptinYeti Platform Enhancement Summary

## Completed Features

### 1. ✅ Website URL Management System
**Status**: Database schema created, models updated, awaiting JavaScript implementation

**What's Done:**
- Added `WebsiteUrl` field to Tenant model for storing user's primary website
- Created `AllowedWebsites` table to track multiple authorized domains per account
- Added website limits to `SubscriptionPlan`:
  - Basic: 1 website
  - Pro: 3 websites
  - Business: 10 websites
  - Enterprise: Unlimited (9999)
- Database schema applied via SQL migration
- Integration page UI updated with "Allowed Websites" section

**What's Needed:**
- JavaScript to manage allowed websites (add/remove/list)
- Backend endpoints in IntegrationController:
  - `GET /Integration/GetAllowedWebsites` - List all websites
  - `POST /Integration/AddAllowedWebsite` - Add new website
  - `DELETE /Integration/RemoveAllowedWebsite/{id}` - Remove website
- Pixel tracking validation to check if website is authorized

### 2. ✅ Landing Page Builder - Foundation
**Status**: Models and controller created, views needed

**What's Done:**
- Created `LandingPage` model with full schema:
  - HTML/CSS content storage
  - Builder configuration (JSON)
  - SEO fields (meta title, description, OG image)
  - Custom domain support (A record configuration)
  - Analytics tracking (views, conversions)
- Created `LandingPageController` with full CRUD operations:
  - Index: List all landing pages
  - Create: New landing page with limits check
  - Edit: Visual builder interface  
  - Save: Auto-save functionality
  - Publish: Toggle publish status
  - View: Public landing page rendering at `/p/{slug}`
- Added landing page limits to subscription plans:
  - Basic: 0 (feature locked)
  - Pro: 5 landing pages
  - Business: 25 landing pages
  - Enterprise: Unlimited
- Database tables created

**What's Needed:**
- Landing page builder UI (drag-drop editor similar to popup designer)
- Index view showing all landing pages
- Public view template with GTM/GA4 integration
- A record configuration interface for custom domains

### 3. ✅ Google Tag Manager & GA4 Integration - Foundation
**Status**: Database fields added, implementation needed

**What's Done:**
- Added fields to Tenant model:
  - `GoogleTagManagerId` (GTM-XXXXXXX format)
  - `GoogleAnalytics4Id` (G-XXXXXXXXXX format)
- Database schema updated
- Landing page public view includes GTM/GA4 IDs in ViewBag

**What's Needed:**
- UI in Integrations page for entering GTM/GA4 IDs
- Backend endpoint to save tracking IDs
- Inject GTM script in `_Layout.cshtml` for account-wide tracking
- Inject GA4 script in `_Layout.cshtml`
- Add GTM/GA4 to popup embed code
- Add GTM/GA4 to landing page public views

### 4. ✅ Onboarding System - Foundation
**Status**: Model created, UI and logic needed

**What's Done:**
- Created `OnboardingProgress` model with 7 steps:
  1. Has added website
  2. Has created popup
  3. Has installed pixel
  4. Has connected integration
  5. Has configured targeting
  6. Has viewed analytics
  7. Has customized branding
- Progress tracking properties (completion percentage, isComplete)
- Database table created with user/tenant relationships

**What's Needed:**
- Onboarding checklist widget for dashboard
- Visual progress indicator (percentage complete)
- Automatic step completion detection
- "Get Started" buttons that link to relevant features
- Dismiss functionality
- Welcome modal for new users

### 5. ⏳ Feature Locking UI Redesign
**Status**: Not started - design needed

**Current Behavior:**
- Features show simple "not available" messages
- No upsell or promotional content

**Required Changes:**
- Replace locked feature access with promotional pages
- Show feature screenshots/demos
- Display benefit lists
- Add "Upgrade to unlock" CTAs
- Allow users to browse locked features (read-only)
- Create upgrade modal with plan comparison

**Affected Features:**
- Landing page builder (locked for Basic plan)
- Advanced targeting (varies by plan)
- API access (higher tiers)
- White label (Enterprise only)
- Additional websites (over plan limit)

### 6. ⏳ Unsplash Stock Image Integration
**Status**: Not started - API integration needed

**Requirements:**
- Register for Unsplash API access
- Add API key to appsettings.json
- Create image search interface in designer
- Add Unsplash browser modal to Media Library
- Implement search by keyword
- Show image attribution (required by Unsplash)
- Download and save selected images to media library
- Track Unsplash API usage

**Implementation Plan:**
1. Install Unsplash API NuGet package or use HttpClient
2. Add "Stock Images" tab to media library
3. Create search UI with grid layout
4. Implement image selection and download
5. Store downloaded images in wwwroot/media
6. Add to MediaLibraryImage database

### 7. ⏳ Homepage Redesign (Figma Implementation)
**Status**: Not started - Figma design review needed

**Requirements:**
- Implement Figma design: https://www.figma.com/design/QWdNA6MIjzBTvmJ41TZvbF/PtinYeti-Wesbite--Copy---1-?node-id=2122-4627&m=dev
- Keep existing OptinYeti logo
- Add integrations section with company logos
- Update Home/Index.cshtml for non-logged-in users
- Keep all existing routes/functionality

**Integration Logos to Add:**
- HubSpot
- Salesforce  
- Mailchimp
- Zapier
- Google Analytics
- ActiveCampaign
- Stripe
- (Add more as available)

## Database Schema Status

### New Tables Created:
1. ✅ `AllowedWebsites` - Tracks authorized domains for pixel tracking
2. ✅ `LandingPages` - Landing page builder content and config
3. ✅ `OnboardingProgress` - User onboarding step completion

### Updated Tables:
1. ✅ `SubscriptionPlans` - Added MaxWebsites, MaxLandingPages, HasLandingPageBuilder
2. ✅ `Tenants` - Added GoogleTagManagerId, GoogleAnalytics4Id

## Next Immediate Steps (Priority Order)

### 1. Complete Website Management (1-2 hours)
- Add JavaScript for allowed websites CRUD in Integration/Index
- Add backend endpoints to IntegrationController
- Load current websites on page load
- Test add/remove functionality
- Update website counter in UI

### 2. Add GTM/GA4 Configuration UI (1 hour)
- Add form fields to Integration page for GTM/GA4 IDs
- Create save endpoint in IntegrationController
- Inject tracking scripts in _Layout.cshtml
- Test tracking on all pages

### 3. Create Onboarding Dashboard Widget (2-3 hours)
- Design checklist UI component
- Add to UserDashboard/Index view
- Implement step completion logic
- Add progress bar visualization
- Connect "Get Started" buttons to features

### 4. Build Landing Page Builder UI (4-6 hours)
- Create Index view (list landing pages)
- Create Edit view with HTML/CSS editor
- Add simple drag-drop builder (or code editor)
- Implement PublicView template
- Test publish/unpublish
- Add custom domain configuration

### 5. Implement Feature Locking Redesign (3-4 hours)
- Audit all feature checks across controllers
- Create promotional pages for locked features
- Design upgrade modal
- Add plan comparison table
- Update navigation to show locked features

### 6. Integrate Unsplash API (2-3 hours)
- Register for Unsplash API
- Install API client or create HttpClient service
- Add stock images tab to media library
- Implement search and download
- Add to popup designer

### 7. Homepage Redesign from Figma (3-5 hours)
- Review Figma design specifications
- Update Home/Index.cshtml
- Add integration logos
- Ensure responsive design
- Test all CTAs and links

## Estimated Total Time: 16-24 hours

## Technical Debt Notes
- Consider adding caching for Unsplash API calls
- Need rate limiting on website additions
- Should add validation for custom domain A records
- GTM/GA4 scripts should be async/defer for performance
- Landing page builder needs autosave functionality
- Onboarding progress should update via background jobs

## Configuration Required
1. Unsplash API credentials (appsettings.json)
2. Custom domain DNS instructions documentation
3. Integration logo image files
4. GTM container setup guide for users

