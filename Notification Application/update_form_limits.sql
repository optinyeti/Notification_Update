-- Update form limits for subscription plans
-- Beginner ($7/month) = 1 form
-- Pro = 5 forms
-- Business = 25 forms
-- Enterprise = unlimited (9999)

UPDATE SubscriptionPlans
SET MaxForms = 1
WHERE Id = 1 AND Name = 'Beginner';

UPDATE SubscriptionPlans
SET MaxForms = 5
WHERE Id = 3 AND Name = 'Pro';

UPDATE SubscriptionPlans
SET MaxForms = 25
WHERE Id = 4 AND Name = 'Business';

UPDATE SubscriptionPlans
SET MaxForms = 9999
WHERE Id = 5 AND Name = 'Enterprise';

-- For any Free plan
UPDATE SubscriptionPlans
SET MaxForms = 1
WHERE Name = 'Free' OR MonthlyPrice = 0;

SELECT Id, Name, MonthlyPrice, MaxForms FROM SubscriptionPlans;
