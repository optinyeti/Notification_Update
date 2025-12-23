# Admin Stripe Configuration

## Overview
Stripe API keys can now be configured directly from the **Admin Settings** page instead of editing `appsettings.json`. This allows each tenant to have their own Stripe account with separate API keys stored securely in the database.

## Features
✅ Configure Stripe keys from Admin panel  
✅ Tenant-specific Stripe accounts support  
✅ Secure storage in database  
✅ Password-masked input fields  
✅ Fallback to appsettings.json for backward compatibility  
✅ Visual setup guide in UI  

## How to Configure

### Step 1: Access Admin Settings
1. Login as an Admin or SuperAdmin
2. Navigate to **Admin → Settings**
3. Scroll down to the **Stripe Payment Configuration** section

### Step 2: Get Your Stripe Keys
1. Click the "Get API Keys" link (opens Stripe Dashboard)
2. Login to your Stripe account at https://dashboard.stripe.com
3. Go to **Developers → API Keys**
4. Copy your keys:
   - **Secret Key** (sk_test_... or sk_live_...)
   - **Publishable Key** (pk_test_... or pk_live_...)

### Step 3: Configure Webhook Secret
1. In Stripe Dashboard, go to **Developers → Webhooks**
2. Click **Add Endpoint**
3. Enter webhook URL: `https://yourdomain.com/Payment/Webhook`
4. Select events:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `checkout.session.completed`
5. Copy the **Signing Secret** (whsec_...)

### Step 4: Save Configuration
1. Paste your Stripe Secret Key in the field (it will be masked)
2. Paste your Stripe Publishable Key
3. Paste your Webhook Secret (it will be masked)
4. Click **Save Changes**

## Field Descriptions

### Secret Key (Required)
- **Format**: `sk_test_...` (test) or `sk_live_...` (production)
- **Purpose**: Server-side API authentication
- **Security**: Stored securely in database, never exposed to clients
- **Note**: Use test keys for development, live keys for production

### Publishable Key
- **Format**: `pk_test_...` (test) or `pk_live_...` (production)
- **Purpose**: Client-side Stripe.js initialization (future use)
- **Security**: Safe to expose to clients
- **Note**: Must match environment of Secret Key (test with test, live with live)

### Webhook Secret
- **Format**: `whsec_...`
- **Purpose**: Validates webhook requests from Stripe
- **Security**: Critical for webhook security
- **Note**: Different secret for each webhook endpoint

## Security Features

### Password-Masked Fields
- Secret Key and Webhook Secret fields are masked by default
- Click the eye icon to toggle visibility
- Helps prevent shoulder-surfing

### Database Encryption
The keys are stored in the Tenants table:
```sql
StripeSecretKey (nullable string)
StripePublishableKey (nullable string)
StripeWebhookSecret (nullable string)
```

**Note**: For production, consider encrypting these fields at the database level or using Azure Key Vault / AWS Secrets Manager.

### Fallback Configuration
If no keys are configured in Admin Settings, the system falls back to `appsettings.json`:
```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "PublishableKey": "pk_test_...",
  "WebhookSecret": "whsec_..."
}
```

## How It Works

### Tenant-Specific Keys
Each tenant can have their own Stripe account:
- Tenant A uses their Stripe account
- Tenant B uses their Stripe account
- Payments go to respective accounts

### Key Resolution Priority
1. **Database (Tenant.StripeSecretKey)** - Checked first
2. **appsettings.json (Stripe:SecretKey)** - Fallback

### StripeService Updates
The `StripeService` now:
1. Retrieves API key from database per-tenant
2. Sets `StripeConfiguration.ApiKey` dynamically for each request
3. Uses tenant-specific webhook secrets for validation
4. Falls back to appsettings if database is empty

## Testing Configuration

### Test Your Setup
1. Save your Stripe keys in Admin Settings
2. Go to `/Payment/Plans`
3. Click "Subscribe Now" on any plan
4. Use test card: `4242 4242 4242 4242`
5. Complete checkout
6. Verify webhook events are received
7. Check subscription status in `/Admin/Subscription`

### Common Issues

**"Invalid API Key" Error**
- Verify key starts with `sk_test_` or `sk_live_`
- Check for extra spaces
- Ensure test/live environments match

**Webhooks Not Working**
- Verify webhook secret is correct
- Check endpoint URL is publicly accessible
- Use Stripe CLI for local testing: `stripe listen --forward-to http://localhost:5117/Payment/Webhook`

**Keys Not Saving**
- Ensure you're logged in as Admin/SuperAdmin
- Check database migration was applied: `dotnet ef database update`
- Verify no validation errors on the form

## Database Migration

The migration `AddStripeConfigToTenant` adds three fields to the Tenants table:

```sql
ALTER TABLE Tenants ADD COLUMN StripeSecretKey TEXT NULL;
ALTER TABLE Tenants ADD COLUMN StripePublishableKey TEXT NULL;
ALTER TABLE Tenants ADD COLUMN StripeWebhookSecret TEXT NULL;
```

Apply with:
```bash
dotnet ef database update
```

## Multi-Tenant Scenarios

### Single Stripe Account (Default)
- Configure keys in `appsettings.json`
- All tenants share one Stripe account
- All payments go to single account
- Good for: SaaS with single business entity

### Multiple Stripe Accounts (Advanced)
- Each tenant configures their own keys in Admin Settings
- Each tenant has separate Stripe account
- Payments go to respective tenant accounts
- Good for: Marketplace platforms, white-label solutions

### Hybrid Approach
- Default keys in `appsettings.json`
- Premium tenants override with their own keys
- Flexible payment routing

## Production Checklist

Before going live:
- [ ] Replace test keys with live keys
- [ ] Configure production webhook endpoint
- [ ] Test checkout with real card (small amount)
- [ ] Verify webhook events update subscription correctly
- [ ] Consider database-level encryption for keys
- [ ] Set up monitoring for failed payments
- [ ] Review Stripe's security best practices

## UI Components

### Visual Indicators
- 🔒 Password masking for sensitive fields
- ⚠️ Security warning about test vs live keys
- ℹ️ Quick setup guide with links
- 🔗 Direct links to Stripe Dashboard
- 👁️ Toggle visibility for masked fields

### Responsive Design
- Mobile-friendly form layout
- Readable font-mono for API keys
- Color-coded sections (purple/blue gradient)
- Icon-enhanced labels

## API Reference

### Get Stripe Secret Key (Internal)
```csharp
private async Task<string> GetStripeSecretKeyAsync(int tenantId)
```
- Returns tenant-specific key or fallback from config
- Throws exception if no key configured

### Get Webhook Secret (Internal)
```csharp
private async Task<string> GetStripeWebhookSecretAsync(int? tenantId = null)
```
- Returns tenant-specific webhook secret or fallback
- Optional tenantId parameter for webhook validation

## Future Enhancements

Potential improvements:
- Encrypt keys using Data Protection API
- Audit log for key changes
- Key rotation support
- Test connection button
- Automatic Stripe account linking via OAuth
- Stripe Connect support for marketplace platforms
- Key expiration warnings

---

**Need Help?** Check [STRIPE-SETUP.md](./STRIPE-SETUP.md) for complete Stripe integration guide.
