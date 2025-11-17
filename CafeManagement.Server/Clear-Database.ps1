# Cafe Management Database Reset Script
param(
    [switch]$Force
)

Write-Host "🔄 Cafe Management Database Cleanup Tool" -ForegroundColor Cyan

$DbPath = "C:\Users\sokar\Cafe-Management\CafeManagement.Server\cafemanagement.db"

if (-not $Force) {
    Write-Host "⚠️  This will delete ALL users, sessions, clients, and usage data!" -ForegroundColor Yellow
    Write-Host "🔧 Database location: $DbPath" -ForegroundColor Gray
    $confirm = Read-Host "Are you sure you want to continue? (y/N)"

    if ($confirm -notmatch "^[Yy]") {
        Write-Host "❌ Operation cancelled." -ForegroundColor Red
        exit 1
    }
}

try {
    Write-Host "🛑 Stopping any running server processes..." -ForegroundColor Yellow

    # Stop any running CafeManagement.Server processes
    Get-Process | Where-Object { $_.ProcessName -like "*CafeManagement*" } | Stop-Process -Force -ErrorAction SilentlyContinue

    Write-Host "⏳ Waiting for processes to stop..." -ForegroundColor Gray
    Start-Sleep -Seconds 3

    # Delete database files
    Write-Host "🗑️  Removing database files..." -ForegroundColor Yellow

    $filesToRemove = @(
        "cafemanagement.db",
        "cafemanagement.db-shm",
        "cafemanagement.db-wal"
    )

    foreach ($file in $filesToRemove) {
        $filePath = Join-Path "C:\Users\sokar\Cafe-Management\CafeManagement.Server" $file
        if (Test-Path $filePath) {
            Remove-Item $filePath -Force
            Write-Host "   ✅ Removed: $file" -ForegroundColor Green
        }
    }

    Write-Host "🔧 Creating fresh database..." -ForegroundColor Yellow

    # Recreate database using EF Core
    Set-Location "C:\Users\sokar\Cafe-Management\CafeManagement.Server"

    # Create new database with seed data
    & dotnet ef database update

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database successfully recreated!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Seed users created:" -ForegroundColor Cyan
        Write-Host "   👤 Admin: username='admin', password='admin123'" -ForegroundColor White
        Write-Host "   👨‍💼 Operator: username='operator', password='operator123'" -ForegroundColor White
        Write-Host ""
        Write-Host "🎯 Database is now clean and ready for fresh login!" -ForegroundColor Green
        Write-Host "🌐 Start the server: dotnet run" -ForegroundColor Gray
    } else {
        Write-Host "❌ Failed to create database. Check the error messages above." -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "✨ Database cleanup completed successfully!" -ForegroundColor Green