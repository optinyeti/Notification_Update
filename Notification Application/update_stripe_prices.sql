-- Update SubscriptionPlans with real Stripe test Price IDs

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = 'price_1Sw5pnPebLxuBVxMGmoYD4E8',
  StripePriceIdYearly = 'price_1Sw5poPebLxuBVxMLL7LIXRM',
  StripeProductId = 'prod_TttdbhU0dZ4O8G'
WHERE Id = 1;

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = 'price_1Sw5ppPebLxuBVxMZe9fcnNb',
  StripePriceIdYearly = 'price_1Sw5ppPebLxuBVxML8KK71mT',
  StripeProductId = 'prod_TttdzJayhg18UO'
WHERE Id = 2;

UPDATE SubscriptionPlans SET 
  StripePriceIdMonthly = 'price_1Sw5pqPebLxuBVxMVthQpuxI',
  StripePriceIdYearly = 'price_1Sw5prPebLxuBVxMj9X8EMUU',
  StripeProductId = 'prod_TttdvXzYFcwqeO'
WHERE Id = 3;
