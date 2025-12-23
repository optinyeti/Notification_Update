# Stripe Payment Integration Setup Guide

## Overview
This application now includes full Stripe payment integration for subscription management. Users can subscribe to plans, manage their billing, and receive automatic subscription updates via webhooks.

## Features Implemented
✅ Stripe Checkout for subscription purchases  
✅ Webhook handling for subscription lifecycle events  
✅ Customer Portal for self-service billing management  
✅ Subscription status tracking (active, past_due, canceled)  
✅ Monthly and yearly billing options  
✅ Automatic subscription renewals  
✅ Payment failure handling  

## Setup Instructions

### 1. Get Your Stripe API Keys
1. Create/login to your Stripe account at https://dashboard.stripe.com
2. Go to **Developers → API Keys**
3. Copy your **Secret Key** (starts with `sk_test_` or `sk_live_`)
4. Copy your **Publishable Key** (starts with `pk_test_` or `pk_live_`)

### 2. Configure Stripe Keys
Open `appsettings.json` and update the Stripe section:

```json
"Stripe": {
  "SecretKey": "sk_test_your_actual_stripe_secret_key_here",
  "PublishableKey": "pk_test_your_actual_publishable_key_here",
  "WebhookSecret": "whsec_your_webhook_secret_here"
}
```

**Note:** For production, create `appsettings.Production.json` with live keys (sk_live_, pk_live_).

### 3. Create Stripe Products & Prices
In your Stripe Dashboard:

1. Go to **Products → Add Product**
2. Create products for each plan (e.g., "Free", "Starter", "Professional", "Enterprise")
3. For each product, create two prices:
   - Monthly price (e.g., $29/month)
   - Yearly price (e.g., $290/year)
4. Copy the **Price IDs** (start with `price_`)

### 4. Update Database with Stripe Price IDs
Run SQL commands to update your subscription plans:

```sql
-- Update Starter Plan
UPDATE SubscriptionPlans 
SET StripePriceIdMonthly = 'price_xxxxxxxxxxxxx',
    StripePriceIdYearly = 'price_yyyyyyyyyyyyy',
    StripeProductId = 'prod_zzzzzzzzzzzz'
WHERE Name = 'Starter';

-- Update Professional Plan
UPDATE SubscriptionPlans 
SET StripePriceIdMonthly = 'price_xxxxxxxxxxxxx',
    StripePriceIdYearly = 'price_yyyyyyyyyyyyy',
    StripeProductId = 'prod_zzzzzzzzzzzz'
WHERE Name = 'Professional';

-- Repeat for other paid plans
```

### 5. Apply Database Migration
Run the migration to add Stripe fields to your database:

```bash
dotnet ef database update
```

This adds the following fields:
- **SubscriptionPlans**: StripePriceIdMonthly, StripePriceIdYearly, StripeProductId
- **Tenants**: StripeCustomerId, StripeSubscriptionId, SubscriptionStartDate, SubscriptionEndDate, SubscriptionStatus

### 6. Configure Stripe Webhooks
Webhooks are essential for subscription updates.

#### For Local Development (Using Stripe CLI):
1. Install Stripe CLI: https://stripe.com/docs/stripe-cli
2. Login: `stripe login`
3. Forward webhooks: `stripe listen --forward-to http://localhost:5117/Payment/Webhook`
4. Copy the webhook signing secret (starts with `whsec_`) to your appsettings.json

#### For Production:
1. Go to Stripe Dashboard → **Developers → Webhooks**
2. Click **Add Endpoint**
3. Enter your webhook URL: `https://yourdomain.com/Payment/Webhook`
4. Select events to listen to:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `checkout.session.completed`
5. Copy the **Signing Secret** to your appsettings.json

### 7. Test the Integration

#### Test Checkout Flow:
1. Run your application
2. Navigate to `/Payment/Plans`
3. Select a plan and click "Subscribe Now"
4. Use Stripe test card: `4242 4242 4242 4242`
   - Expiry: Any future date
   - CVC: Any 3 digits
   - ZIP: Any 5 digits
5. Complete payment
6. Verify you're redirected to subscription page with active status

#### Test Webhooks:
With Stripe CLI running:
1. Complete a test checkout
2. Watch the CLI output for webhook events
3. Check database to verify tenant subscription fields are updated
4. Visit `/Admin/Subscription` to see subscription status

## Architecture Overview

### Components Created

#### 1. **StripeService** (`Services/StripeService.cs`)
- `CreateCheckoutSessionAsync()` - Creates Stripe Checkout session
- `HandleWebhookEventAsync()` - Processes webhook events
- `CreateCustomerPortalSessionAsync()` - Generates billing portal link
- `CancelSubscriptionAsync()` - Cancels active subscriptions

#### 2. **PaymentController** (`Controllers/PaymentController.cs`)
- `Plans()` - Displays subscription plans
- `CreateCheckoutSession()` - Redirects to Stripe Checkout
- `Success()` - Post-payment success page
- `Webhook()` - Receives Stripe webhook events (AllowAnonymous)
- `ManageBilling()` - Opens Stripe Customer Portal
- `CancelSubscription()` - Handles cancellation requests

#### 3. **Views**
- `/Views/Payment/Plans.cshtml` - Subscription plan selection UI with monthly/yearly toggle
- `/Views/Admin/Subscription.cshtml` - Enhanced with Stripe status badges and "Manage Billing" button

### Webhook Events Handled
The system automatically handles these Stripe events:

- **checkout.session.completed** - Initial subscription created after successful checkout
- **customer.subscription.created** - Subscription created
- **customer.subscription.updated** - Subscription modified (plan change, renewal, etc.)
- **customer.subscription.deleted** - Subscription canceled or expired
- **invoice.payment_succeeded** - Successful payment (renewal)
- **invoice.payment_failed** - Failed payment (updates status to past_due)

All webhook events automatically update the tenant's subscription status, dates, and Stripe IDs.

## Subscription Flow

### New Subscription:
1. User clicks "Subscribe Now" on /Payment/Plans
2. System creates Stripe Checkout Session with metadata (tenant_id, plan_id, billing_period)
3. User redirected to Stripe Checkout
4. User completes payment
5. Stripe sends `checkout.session.completed` webhook
6. System creates Stripe Customer and Subscription
7. Tenant record updated with StripeCustomerId, StripeSubscriptionId, status="active"
8. User redirected to success page

### Subscription Management:
1. User clicks "Manage Billing" button in /Admin/Subscription
2. System creates Customer Portal session
3. User redirected to Stripe Customer Portal
4. User can update payment method, view invoices, or cancel subscription
5. Any changes trigger webhooks that update tenant record automatically

### Payment Failures:
1. Stripe attempts to charge card
2. If payment fails, `invoice.payment_failed` webhook sent
3. System updates tenant SubscriptionStatus to "past_due"
4. User sees warning in subscription page
5. User can update payment method via Customer Portal

## Testing Stripe Integration

### Test Cards:
```
Success: 4242 4242 4242 4242
Decline: 4000 0000 0000 0002
Requires 3DS: 4000 0027 6000 3184
```

### Test Webhook Events:
Using Stripe CLI, trigger test events:
```bash
stripe trigger customer.subscription.created
stripe trigger customer.subscription.updated
stripe trigger invoice.payment_succeeded
stripe trigger invoice.payment_failed
```

## Security Notes
- ✅ Webhook endpoint validates Stripe signatures using webhook secret
- ✅ Secret keys stored in appsettings.json (add to .gitignore)
- ✅ Checkout sessions include metadata for security
- ✅ Customer Portal sessions auto-expire
- ✅ All payment processing happens on Stripe (PCI compliant)

## Troubleshooting

### "Invalid API Key" Error:
- Verify your Secret Key in appsettings.json starts with `sk_test_` or `sk_live_`
- Ensure no extra spaces in the key
- Make sure you're using the correct environment (test vs live)

### Webhooks Not Working:
- Check webhook secret matches in appsettings.json
- Verify endpoint URL is correct in Stripe Dashboard
- Check application logs for webhook errors
- Use Stripe CLI `stripe listen` for local testing
- Ensure webhook endpoint is publicly accessible (for production)

### Subscription Not Updating:
- Check Stripe Dashboard → Events for webhook delivery status
- Verify webhook endpoint returned 200 status
- Check application logs for errors in HandleWebhookEventAsync
- Ensure database migration was applied (Stripe fields exist)

### Customer Portal Not Opening:
- Verify StripeCustomerId exists in Tenant record
- Check that return_url is correct
- Ensure Secret Key has correct permissions

## Production Checklist
Before going live:

- [ ] Replace test API keys with live keys in production appsettings
- [ ] Create live Stripe products and update database with live price IDs
- [ ] Configure production webhook endpoint in Stripe Dashboard
- [ ] Test checkout flow with real card (small amount)
- [ ] Verify webhook events are received and processed
- [ ] Enable Stripe billing email notifications
- [ ] Set up monitoring for failed payments
- [ ] Configure Stripe tax settings if applicable
- [ ] Review Stripe's security best practices

## Resources
- Stripe Documentation: https://stripe.com/docs
- Stripe Testing: https://stripe.com/docs/testing
- Webhook Events: https://stripe.com/docs/api/events
- Customer Portal: https://stripe.com/docs/billing/subscriptions/customer-portal
- Stripe CLI: https://stripe.com/docs/stripe-cli

---

**Need Help?** Contact support@popupmanager.com or check Stripe's support at https://support.stripe.com
