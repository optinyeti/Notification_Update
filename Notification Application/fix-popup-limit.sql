-- Fix Popup Limit Issue
-- This script provides several solutions to fix the popup limit exceeded error

-- OPTION 1: Sync the popup count with the actual number of popups in the database
-- Run this first to check if the count is out of sync
UPDATE Tenants
SET PopupCount = (
    SELECT COUNT(*) 
    FROM Popups 
    WHERE Popups.TenantId = Tenants.Id
);

-- OPTION 2: Check current usage
-- See what the actual counts are
SELECT 
    t.Id,
    t.Name,
    t.PopupCount as TrackedCount,
    (SELECT COUNT(*) FROM Popups WHERE TenantId = t.Id) as ActualCount,
    sp.Name as PlanName,
    sp.MaxPopups,
    CASE 
        WHEN sp.MaxPopups = -1 THEN 'Unlimited'
        WHEN t.PopupCount >= sp.MaxPopups THEN 'LIMIT REACHED'
        ELSE 'OK'
    END as Status
FROM Tenants t
INNER JOIN SubscriptionPlans sp ON t.SubscriptionPlanId = sp.Id;

-- OPTION 3: Upgrade to Enterprise plan (unlimited popups)
-- Replace TenantId = 1 with your actual tenant ID
UPDATE Tenants 
SET SubscriptionPlanId = 3 
WHERE Id = 1;

-- OPTION 4: Increase the limit for Professional plan (temporary fix)
-- This increases the Professional plan limit from 25 to 100
UPDATE SubscriptionPlans 
SET MaxPopups = 100 
WHERE Id = 2;

-- OPTION 5: Reset popup count to zero (use with caution)
-- Replace TenantId = 1 with your actual tenant ID
-- UPDATE Tenants 
-- SET PopupCount = 0 
-- WHERE Id = 1;

-- OPTION 6: Delete old popups to free up space
-- This will delete draft popups older than 30 days
-- Uncomment and run if you want to clean up old drafts
-- DELETE FROM Popups 
-- WHERE Status = 0 
-- AND CreatedAt < datetime('now', '-30 days');

-- After deleting, sync the count again
-- UPDATE Tenants
-- SET PopupCount = (
--     SELECT COUNT(*) 
--     FROM Popups 
--     WHERE Popups.TenantId = Tenants.Id
-- );
