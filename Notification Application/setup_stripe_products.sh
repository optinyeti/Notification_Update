#!/bin/bash

# Stripe Test API Key
STRIPE_KEY="sk_test_51StvTLPebLxuBVxMeWOvCiIj6k360Eds3GFBBMMci2NWwAyWcaSp8JLTPuzMNrefK8h8b0APlaI8FL62Nrl4swJY00LcaG7v0x"

echo "Creating Stripe Test Products and Prices..."
echo "==========================================="

# Create Free Plan Product
echo -e "\n1. Creating Free Plan Product..."
FREE_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "${STRIPE_KEY}:" \
  -d name="Free Plan" \
  -d description="Perfect for getting started - 3 popups, 1K views/month")

FREE_PRODUCT_ID=$(echo $FREE_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Free Product ID: $FREE_PRODUCT_ID"

# Create Free Plan Monthly Price (FREE)
echo "Creating Free Monthly Price..."
FREE_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$FREE_PRODUCT_ID" \
  -d unit_amount=0 \
  -d currency=usd \
  -d "recurring[interval]"=month)

FREE_MONTHLY_ID=$(echo $FREE_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Free Monthly Price ID: $FREE_MONTHLY_ID"

# Create Free Plan Yearly Price (FREE)
echo "Creating Free Yearly Price..."
FREE_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$FREE_PRODUCT_ID" \
  -d unit_amount=0 \
  -d currency=usd \
  -d "recurring[interval]"=year)

FREE_YEARLY_ID=$(echo $FREE_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Free Yearly Price ID: $FREE_YEARLY_ID"

# Create Professional Plan Product
echo -e "\n2. Creating Professional Plan Product..."
PRO_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "${STRIPE_KEY}:" \
  -d name="Professional Plan" \
  -d description="For growing businesses - 25 popups, 50K views/month")

PRO_PRODUCT_ID=$(echo $PRO_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Professional Product ID: $PRO_PRODUCT_ID"

# Create Professional Plan Monthly Price ($29)
echo "Creating Professional Monthly Price..."
PRO_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$PRO_PRODUCT_ID" \
  -d unit_amount=2900 \
  -d currency=usd \
  -d "recurring[interval]"=month)

PRO_MONTHLY_ID=$(echo $PRO_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Professional Monthly Price ID: $PRO_MONTHLY_ID"

# Create Professional Plan Yearly Price ($290)
echo "Creating Professional Yearly Price..."
PRO_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$PRO_PRODUCT_ID" \
  -d unit_amount=29000 \
  -d currency=usd \
  -d "recurring[interval]"=year)

PRO_YEARLY_ID=$(echo $PRO_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Professional Yearly Price ID: $PRO_YEARLY_ID"

# Create Enterprise Plan Product
echo -e "\n3. Creating Enterprise Plan Product..."
ENTERPRISE_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "${STRIPE_KEY}:" \
  -d name="Enterprise Plan" \
  -d description="For large organizations - Unlimited popups and views")

ENTERPRISE_PRODUCT_ID=$(echo $ENTERPRISE_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Enterprise Product ID: $ENTERPRISE_PRODUCT_ID"

# Create Enterprise Plan Monthly Price ($99)
echo "Creating Enterprise Monthly Price..."
ENTERPRISE_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$ENTERPRISE_PRODUCT_ID" \
  -d unit_amount=9900 \
  -d currency=usd \
  -d "recurring[interval]"=month)

ENTERPRISE_MONTHLY_ID=$(echo $ENTERPRISE_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Enterprise Monthly Price ID: $ENTERPRISE_MONTHLY_ID"

# Create Enterprise Plan Yearly Price ($990)
echo "Creating Enterprise Yearly Price..."
ENTERPRISE_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "${STRIPE_KEY}:" \
  -d product="$ENTERPRISE_PRODUCT_ID" \
  -d unit_amount=99000 \
  -d currency=usd \
  -d "recurring[interval]"=year)

ENTERPRISE_YEARLY_ID=$(echo $ENTERPRISE_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "Enterprise Yearly Price ID: $ENTERPRISE_YEARLY_ID"

# Generate SQL Update Script
echo -e "\n==========================================="
echo "Generating SQL Update Script..."
echo "==========================================="

cat > update_stripe_prices.sql << EOF
-- Update SubscriptionPlans with real Stripe test Price IDs
-- Generated on $(date)

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = '$FREE_MONTHLY_ID',
  StripePriceIdYearly = '$FREE_YEARLY_ID',
  StripeProductId = '$FREE_PRODUCT_ID'
WHERE Id = 1;

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = '$PRO_MONTHLY_ID',
  StripePriceIdYearly = '$PRO_YEARLY_ID',
  StripeProductId = '$PRO_PRODUCT_ID'
WHERE Id = 2;

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = '$ENTERPRISE_MONTHLY_ID',
  StripePriceIdYearly = '$ENTERPRISE_YEARLY_ID',
  StripeProductId = '$ENTERPRISE_PRODUCT_ID'
WHERE Id = 3;
EOF

echo -e "\n✓ SQL file created: update_stripe_prices.sql"

echo -e "\n==========================================="
echo "Summary of Created IDs:"
echo "==========================================="
echo "FREE Plan:"
echo "  Product: $FREE_PRODUCT_ID"
echo "  Monthly: $FREE_MONTHLY_ID"
echo "  Yearly:  $FREE_YEARLY_ID"
echo ""
echo "PROFESSIONAL Plan:"
echo "  Product: $PRO_PRODUCT_ID"
echo "  Monthly: $PRO_MONTHLY_ID"
echo "  Yearly:  $PRO_YEARLY_ID"
echo ""
echo "ENTERPRISE Plan:"
echo "  Product: $ENTERPRISE_PRODUCT_ID"
echo "  Monthly: $ENTERPRISE_MONTHLY_ID"
echo "  Yearly:  $ENTERPRISE_YEARLY_ID"
echo ""
echo "Now run: sqlite3 PopupManager.db < update_stripe_prices.sql"
