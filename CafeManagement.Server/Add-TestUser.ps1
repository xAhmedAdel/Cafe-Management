# Add Test User Script
param(
    [string]$Username = "testuser",
    [string]$Password = "testuser123",
    [string]$Role = "Customer",
    [decimal]$Balance = 25.00
)

Write-Host "Adding test user to Cafe Management database..." -ForegroundColor Cyan

try {
    # Hash the password (using the same hash as seed data for simplicity)
    $passwordHash = "AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q="

    # SQL to insert new user
    $sql = @"
INSERT INTO Users (Username, PasswordHash, Email, Role, Balance, CreatedAt, UpdatedAt, AvailableMinutes, LastLoginTime, IsActive)
VALUES ('$Username', '$passwordHash', '$Username@cafemanagement.com', $Role, $Balance, datetime('now'), datetime('now'), 0, NULL, 1);
"@

    # Execute SQL using SQLite command line
    $sqlitePath = Get-Command sqlite3 -ErrorAction SilentlyContinue
    if ($sqlitePath) {
        Set-Location "C:\Users\sokar\Cafe-Management\CafeManagement.Server"
        $result = & $sqlitePath.Path cafemanagement.db $sql

        Write-Host "Successfully added user: $Username" -ForegroundColor Green
        Write-Host "Username: $Username" -ForegroundColor White
        Write-Host "Password: $Password" -ForegroundColor White
        Write-Host "Role: $Role" -ForegroundColor White
        Write-Host "Balance: $$Balance" -ForegroundColor White
    } else {
        Write-Host "SQLite3 command not found. Please install SQLite tools." -ForegroundColor Red
    }

} catch {
    Write-Host "Error adding user: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "User addition process completed." -ForegroundColor Gray