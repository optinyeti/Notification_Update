#!/bin/bash

# Stripe Product and Price Setup Script
# Creates products and prices for new subscription plans

# IMPORTANT: Set your Stripe Secret Key here or export as environment variable
# export STRIPE_SECRET_KEY="sk_test_..."

STRIPE_KEY="${STRIPE_SECRET_KEY:-YOUR_STRIPE_SECRET_KEY_HERE}"

if [ "$STRIPE_KEY" == "YOUR_STRIPE_SECRET_KEY_HERE" ]; then
    echo "❌ Error: Please set your STRIPE_SECRET_KEY environment variable"
    echo "   export STRIPE_SECRET_KEY='sk_test_...'"
    exit 1
fi

echo "🚀 Creating Stripe Products and Prices..."
echo "=========================================="

# Create Starter Plan Product
echo ""
echo "📦 Creating Starter Plan..."
STARTER_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "$STRIPE_KEY:" \
  -d name="Starter Plan" \
  -d description="Everything you need to start growing" \
  -d metadata[plan_id]="2" \
  -d metadata[features]="3 Popups, 10K Views, 5 Forms, 3 Landing Pages, CRM")

STARTER_PRODUCT_ID=$(echo $STARTER_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)

if [ -z "$STARTER_PRODUCT_ID" ]; then
    echo "❌ Failed to create Starter product"
    echo "$STARTER_PRODUCT"
    exit 1
fi

echo "✅ Starter Product ID: $STARTER_PRODUCT_ID"

# Create Starter Monthly Price
echo "   Creating monthly price ($29)..."
STARTER_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$STARTER_PRODUCT_ID" \
  -d unit_amount=2900 \
  -d currency=usd \
  -d "recurring[interval]"=month \
  -d nickname="Starter Monthly")

STARTER_MONTHLY_ID=$(echo $STARTER_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Monthly Price ID: $STARTER_MONTHLY_ID"

# Create Starter Yearly Price
echo "   Creating yearly price ($288 = $24/mo)..."
STARTER_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$STARTER_PRODUCT_ID" \
  -d unit_amount=28800 \
  -d currency=usd \
  -d "recurring[interval]"=year \
  -d nickname="Starter Yearly")

STARTER_YEARLY_ID=$(echo $STARTER_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Yearly Price ID: $STARTER_YEARLY_ID"

# Create Professional Plan Product
echo ""
echo "📦 Creating Professional Plan..."
PRO_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "$STRIPE_KEY:" \
  -d name="Professional Plan" \
  -d description="AI-powered growth for serious businesses" \
  -d metadata[plan_id]="3" \
  -d metadata[features]="15 Popups, 100K Views, AI Features, 25 Forms, 25 Landing Pages, White Label" \
  -d metadata[popular]="true")

PRO_PRODUCT_ID=$(echo $PRO_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)

if [ -z "$PRO_PRODUCT_ID" ]; then
    echo "❌ Failed to create Professional product"
    echo "$PRO_PRODUCT"
    exit 1
fi

echo "✅ Professional Product ID: $PRO_PRODUCT_ID"

# Create Professional Monthly Price
echo "   Creating monthly price ($79)..."
PRO_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$PRO_PRODUCT_ID" \
  -d unit_amount=7900 \
  -d currency=usd \
  -d "recurring[interval]"=month \
  -d nickname="Professional Monthly")

PRO_MONTHLY_ID=$(echo $PRO_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Monthly Price ID: $PRO_MONTHLY_ID"

# Create Professional Yearly Price
echo "   Creating yearly price ($804 = $67/mo)..."
PRO_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$PRO_PRODUCT_ID" \
  -d unit_amount=80400 \
  -d currency=usd \
  -d "recurring[interval]"=year \
  -d nickname="Professional Yearly")

PRO_YEARLY_ID=$(echo $PRO_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Yearly Price ID: $PRO_YEARLY_ID"

# Create Enterprise Plan Product
echo ""
echo "📦 Creating Enterprise Plan..."
ENTERPRISE_PRODUCT=$(curl -s https://api.stripe.com/v1/products \
  -u "$STRIPE_KEY:" \
  -d name="Enterprise Plan" \
  -d description="Unlimited power for large organizations" \
  -d metadata[plan_id]="4" \
  -d metadata[features]="Unlimited Everything, Priority Support, Custom Integrations")

ENTERPRISE_PRODUCT_ID=$(echo $ENTERPRISE_PRODUCT | grep -o '"id":"prod_[^"]*"' | head -1 | cut -d'"' -f4)

if [ -z "$ENTERPRISE_PRODUCT_ID" ]; then
    echo "❌ Failed to create Enterprise product"
    echo "$ENTERPRISE_PRODUCT"
    exit 1
fi

echo "✅ Enterprise Product ID: $ENTERPRISE_PRODUCT_ID"

# Create Enterprise Monthly Price
echo "   Creating monthly price ($199)..."
ENTERPRISE_MONTHLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$ENTERPRISE_PRODUCT_ID" \
  -d unit_amount=19900 \
  -d currency=usd \
  -d "recurring[interval]"=month \
  -d nickname="Enterprise Monthly")

ENTERPRISE_MONTHLY_ID=$(echo $ENTERPRISE_MONTHLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Monthly Price ID: $ENTERPRISE_MONTHLY_ID"

# Create Enterprise Yearly Price
echo "   Creating yearly price ($2028 = $169/mo)..."
ENTERPRISE_YEARLY=$(curl -s https://api.stripe.com/v1/prices \
  -u "$STRIPE_KEY:" \
  -d product="$ENTERPRISE_PRODUCT_ID" \
  -d unit_amount=202800 \
  -d currency=usd \
  -d "recurring[interval]"=year \
  -d nickname="Enterprise Yearly")

ENTERPRISE_YEARLY_ID=$(echo $ENTERPRISE_YEARLY | grep -o '"id":"price_[^"]*"' | head -1 | cut -d'"' -f4)
echo "   ✅ Yearly Price ID: $ENTERPRISE_YEARLY_ID"

# Summary
echo ""
echo "=========================================="
echo "✅ All Stripe Products and Prices Created!"
echo "=========================================="
echo ""
echo "📝 Add these IDs to your appsettings.json or database:"
echo ""
echo "STARTER PLAN:"
echo "  Product ID: $STARTER_PRODUCT_ID"
echo "  Monthly Price ID: $STARTER_MONTHLY_ID"
echo "  Yearly Price ID: $STARTER_YEARLY_ID"
echo ""
echo "PROFESSIONAL PLAN:"
echo "  Product ID: $PRO_PRODUCT_ID"
echo "  Monthly Price ID: $PRO_MONTHLY_ID"
echo "  Yearly Price ID: $PRO_YEARLY_ID"
echo ""
echo "ENTERPRISE PLAN:"
echo "  Product ID: $ENTERPRISE_PRODUCT_ID"
echo "  Monthly Price ID: $ENTERPRISE_MONTHLY_ID"
echo "  Yearly Price ID: $ENTERPRISE_YEARLY_ID"
echo ""
echo "📋 SQL Update Script:"
echo "----------------------------------------"
echo "UPDATE SubscriptionPlans SET"
echo "  StripeProductId = '$STARTER_PRODUCT_ID',"
echo "  StripePriceIdMonthly = '$STARTER_MONTHLY_ID',"
echo "  StripePriceIdYearly = '$STARTER_YEARLY_ID'"
echo "WHERE Id = 2;"
echo ""
echo "UPDATE SubscriptionPlans SET"
echo "  StripeProductId = '$PRO_PRODUCT_ID',"
echo "  StripePriceIdMonthly = '$PRO_MONTHLY_ID',"
echo "  StripePriceIdYearly = '$PRO_YEARLY_ID'"
echo "WHERE Id = 3;"
echo ""
echo "UPDATE SubscriptionPlans SET"
echo "  StripeProductId = '$ENTERPRISE_PRODUCT_ID',"
echo "  StripePriceIdMonthly = '$ENTERPRISE_MONTHLY_ID',"
echo "  StripePriceIdYearly = '$ENTERPRISE_YEARLY_ID'"
echo "WHERE Id = 4;"
echo "----------------------------------------"
echo ""
echo "🎉 Setup Complete! Update your database with the SQL above."
