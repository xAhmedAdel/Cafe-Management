-- Verify database contents
SELECT '=== USERS ===' as info;
SELECT Id, Username, Email, Role, Balance, IsActive FROM Users;

SELECT '';
SELECT '=== SESSIONS ===' as info;
SELECT COUNT(*) as SessionCount FROM Sessions;

SELECT '';
SELECT '=== CLIENTS ===' as info;
SELECT COUNT(*) as ClientCount FROM Clients;

SELECT '';
SELECT '=== USAGE LOGS ===' as info;
SELECT COUNT(*) as UsageLogCount FROM UsageLogs;