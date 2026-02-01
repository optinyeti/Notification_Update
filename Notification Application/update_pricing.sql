-- Update SubscriptionPlans with new pricing and Stripe Price IDs

UPDATE SubscriptionPlans SET 
  MonthlyPrice = 7,
  YearlyPrice = 70,
  Description = 'Best for individuals just getting started',
  MaxPopups = 10,
  MaxPopupViews = 10000,
  StripePriceIdMonthly = 'price_1Sw6SpPebLxuBVxMTHAUCHV4',
  StripePriceIdYearly = 'price_1Sw6SpPebLxuBVxMAhimIt2l',
  StripeProductId = 'prod_TtuHQflxTTI2Jm'
WHERE Id = 1;

UPDATE SubscriptionPlans SET 
  MonthlyPrice = 17,
  YearlyPrice = 170,
  Description = 'Best for growing businesses and creators',
  MaxPopups = 25,
  MaxPopupViews = 50000,
  StripePriceIdMonthly = 'price_1Sw6SqPebLxuBVxMVvkuOYsU',
  StripePriceIdYearly = 'price_1Sw6SrPebLxuBVxM2BN0wiFh',
  StripeProductId = 'prod_TtuHYDYMk0TXry'
WHERE Id = 2;

UPDATE SubscriptionPlans SET 
  Name = 'Pro Plan',
  MonthlyPrice = 25,
  YearlyPrice = 250,
  Description = 'Best for professionals and power users',
  MaxPopups = -1,
  MaxPopupViews = -1,
  MaxUsers = 5,
  StripePriceIdMonthly = 'price_1Sw6SsPebLxuBVxMzMgjeHY5',
  StripePriceIdYearly = 'price_1Sw6SsPebLxuBVxMpnlHy51s',
  StripeProductId = 'prod_TtuHMseGJFropk'
WHERE Id = 3;

UPDATE SubscriptionPlans SET 
  Name = 'Growth Plan',
  MonthlyPrice = 37,
  YearlyPrice = 370,
  Description = 'Best for teams and scaling businesses',
  StripePriceIdMonthly = 'price_1Sw6StPebLxuBVxMBnxOH4nh',
  StripePriceIdYearly = 'price_1Sw6SuPebLxuBVxMRELv7oAO',
  StripeProductId = 'prod_TtuHmw9koR7coL'
WHERE Id = 4;
