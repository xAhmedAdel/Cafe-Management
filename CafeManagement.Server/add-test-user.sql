-- Add test customer user
INSERT INTO Users (Username, PasswordHash, Email, Role, Balance, CreatedAt, UpdatedAt, AvailableMinutes, LastLoginTime, IsActive)
VALUES (
    'testuser',
    'AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q=', -- testuser123
    'testuser@cafemanagement.com',
    2, -- Customer role
    25.00,
    datetime('now'),
    datetime('now'),
    0,
    NULL,
    1
);