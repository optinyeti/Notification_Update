# Stripe Price IDs Configuration

## Summary
Successfully configured Stripe test products and prices for all subscription plans.

**Date Configured:** February 1, 2026

## Stripe Test API Keys
```
Publishable Key: pk_test_51StvTLPebLxuBVxMUySGfO4WIqbZ0QgqAkdUZavcFlB7Indib6GN5BJBL7OWlUe5mlmzy32t6yamdTSp8RKGW4Bq00VemF4OFp

Secret Key: sk_test_51StvTLPebLxuBVxMeWOvCiIj6k360Eds3GFBBMMci2NWwAyWcaSp8JLTPuzMNrefK8h8b0APlaI8FL62Nrl4swJY00LcaG7v0x
```

## Created Products & Prices

### 1. FREE PLAN
- **Product ID:** `prod_TttdbhU0dZ4O8G`
- **Monthly Price ID:** `price_1Sw5pnPebLxuBVxMGmoYD4E8` ($0/month)
- **Yearly Price ID:** `price_1Sw5poPebLxuBVxMLL7LIXRM` ($0/year)
- **Features:** 3 popups, 1K views/month

### 2. PROFESSIONAL PLAN
- **Product ID:** `prod_TttdzJayhg18UO`
- **Monthly Price ID:** `price_1Sw5ppPebLxuBVxMZe9fcnNb` ($29/month)
- **Yearly Price ID:** `price_1Sw5ppPebLxuBVxML8KK71mT` ($290/year)
- **Features:** 25 popups, 50K views/month, Advanced targeting, Analytics, API access

### 3. ENTERPRISE PLAN
- **Product ID:** `prod_TttdvXzYFcwqeO`
- **Monthly Price ID:** `price_1Sw5pqPebLxuBVxMVthQpuxI` ($99/month)
- **Yearly Price ID:** `price_1Sw5prPebLxuBVxMj9X8EMUU` ($990/year)
- **Features:** Unlimited popups, Unlimited views, All features, Priority support, White label

## Database Status
✅ All subscription plans have been updated with Stripe Price IDs
✅ Database file: `PopupManager.db`

## Testing the Integration

### 1. Visit the Plans Page
Navigate to: https://curly-space-disco-5v4wv65469xc476r-5117.app.github.dev/Payment/Plans

### 2. Test Checkout Flow
- Click "Upgrade" on any plan
- You'll be redirected to Stripe Checkout (test mode)
- Use test card: `4242 4242 4242 4242`
- Any future expiry date
- Any 3-digit CVC

### 3. Test Cards
```
✅ Success: 4242 4242 4242 4242
❌ Decline: 4000 0000 0000 0002
🔄 3D Secure: 4000 0025 0000 3155
```

## Webhook Configuration
If you need webhooks for subscription events:

1. Go to Stripe Dashboard → Developers → Webhooks
2. Add endpoint: `https://your-domain/Payment/Webhook`
3. Select events:
   - `checkout.session.completed`
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
4. Copy webhook signing secret to appsettings.json:
   ```json
   "Stripe": {
     "WebhookSecret": "whsec_your_webhook_secret_here"
   }
   ```

## View in Stripe Dashboard
- Products: https://dashboard.stripe.com/test/products
- Prices: https://dashboard.stripe.com/test/prices
- Customers: https://dashboard.stripe.com/test/customers
- Subscriptions: https://dashboard.stripe.com/test/subscriptions

## Notes
- These are **TEST** prices (won't charge real cards)
- All amounts are in USD
- Subscriptions auto-renew until canceled
- Customers can manage subscriptions via Billing Portal

## Troubleshooting

### "Price ID not configured" Error
✅ FIXED - Price IDs now configured in database

### Webhook not working
- Verify webhook secret in appsettings.json
- Check Stripe Dashboard → Webhooks for delivery logs
- Ensure endpoint is publicly accessible

### Payment fails
- Confirm using test mode keys (start with `pk_test_` and `sk_test_`)
- Use test card numbers from Stripe documentation
- Check Stripe logs for detailed error messages
