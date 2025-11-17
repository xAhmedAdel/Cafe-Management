-- Cafe Management Database Reset Script
-- This script clears all users, sessions, and related data while preserving basic configuration

-- Disable foreign key constraints temporarily
PRAGMA foreign_keys = OFF;

-- Clear all session-related data first (due to foreign key constraints)
DELETE FROM UsageLogs;
DELETE FROM Sessions;
DELETE FROM ClientDeployments;
DELETE FROM DeploymentLogs;
DELETE FROM LockScreenConfigs;

-- Clear all clients
DELETE FROM Clients;

-- Clear all users except for seeding (we'll re-create them)
DELETE FROM Users;

-- Reset auto-increment counters
DELETE FROM sqlite_sequence WHERE name IN ('Users', 'Clients', 'Sessions', 'UsageLogs', 'LockScreenConfigs', 'ClientDeployments', 'DeploymentLogs');

-- Re-enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Insert fresh seed data
INSERT INTO Users (Id, Username, PasswordHash, Email, Role, Balance, CreatedAt, UpdatedAt) VALUES
(1, 'admin', 'AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q=', 'admin@cafemanagement.com', 'Admin', 1000.00, '2025-01-01 00:00:00', '2025-01-01 00:00:00'),
(2, 'testuser', 'AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q=', 'test@cafemanagement.com', 'User', 50.00, '2025-01-01 00:00:00', '2025-01-01 00:00:00');

-- Keep billing settings (they should already exist from seed data)
INSERT OR IGNORE INTO BillingSettings (Id, HourlyRate, Currency, MinimumSessionDuration, RoundUpToNearestHour, Description, IsActive, CreatedAt, UpdatedAt)
VALUES (1, 20.00, 'L.E', '1 hour', 1, 'Default billing configuration', 1, '2025-01-01 00:00:00', '2025-01-01 00:00:00');

COMMIT;