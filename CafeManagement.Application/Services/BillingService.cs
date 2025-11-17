using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;

namespace CafeManagement.Application.Services;

public interface IBillingService
{
    Task<decimal> GetHourlyRateAsync();
    Task<decimal> CalculateSessionCostAsync(int durationMinutes);
    Task<decimal> CalculateSessionCostAsync(DateTime startTime, DateTime? endTime);
    Task<int> CalculateSessionMinutesAsync(DateTime startTime, DateTime? endTime);
    Task<decimal> RoundToNearestHourAsync(decimal amount);
    Task<BillingSettings?> GetCurrentSettingsAsync();
    Task<BillingSettings> UpdateSettingsAsync(decimal hourlyRate, string currency = "L.E");
}

public class BillingService : IBillingService
{
    private readonly ILogger<BillingService> _logger;
    private readonly IApplicationDbContext _context;
    private const decimal DEFAULT_HOURLY_RATE = 20.00m; // 20 L.E per hour

    public BillingService(ILogger<BillingService> logger, IApplicationDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<decimal> GetHourlyRateAsync()
    {
        try
        {
            var settings = await _context.BillingSettings
                .FirstOrDefaultAsync(s => s.IsActive);

            return settings?.HourlyRate ?? DEFAULT_HOURLY_RATE;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hourly rate from database");
            return DEFAULT_HOURLY_RATE;
        }
    }

    public async Task<decimal> CalculateSessionCostAsync(int durationMinutes)
    {
        if (durationMinutes <= 0)
            return 0;

        var hourlyRate = await GetHourlyRateAsync();
        var hours = Math.Ceiling((decimal)durationMinutes / 60);
        return hours * hourlyRate;
    }

    public async Task<decimal> CalculateSessionCostAsync(DateTime startTime, DateTime? endTime)
    {
        if (!endTime.HasValue || endTime.Value <= startTime)
            return 0;

        var durationMinutes = await CalculateSessionMinutesAsync(startTime, endTime);
        return await CalculateSessionCostAsync(durationMinutes);
    }

    public async Task<int> CalculateSessionMinutesAsync(DateTime startTime, DateTime? endTime)
    {
        if (!endTime.HasValue || endTime.Value <= startTime)
            return 0;

        var settings = await GetCurrentSettingsAsync();
        if (settings?.RoundUpToNearestHour == true)
        {
            return (int)Math.Ceiling((endTime.Value - startTime).TotalMinutes);
        }
        else
        {
            return (int)Math.Floor((endTime.Value - startTime).TotalMinutes);
        }
    }

    public async Task<decimal> RoundToNearestHourAsync(decimal amount)
    {
        var settings = await GetCurrentSettingsAsync();
        if (settings?.RoundUpToNearestHour == true)
        {
            return Math.Ceiling(amount);
        }
        else
        {
            return Math.Floor(amount);
        }
    }

    public async Task<BillingSettings?> GetCurrentSettingsAsync()
    {
        try
        {
            return await _context.BillingSettings
                .FirstOrDefaultAsync(s => s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing settings from database");
            return null;
        }
    }

    public async Task<BillingSettings> UpdateSettingsAsync(decimal hourlyRate, string currency = "L.E")
    {
        try
        {
            var settings = await GetCurrentSettingsAsync();

            if (settings == null)
            {
                settings = new BillingSettings
                {
                    HourlyRate = hourlyRate,
                    Currency = currency,
                    IsActive = true,
                    Description = "Updated via admin interface",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.BillingSettings.Add(settings);
            }
            else
            {
                settings.HourlyRate = hourlyRate;
                settings.Currency = currency;
                settings.UpdatedAt = DateTime.UtcNow;
                settings.Description = "Updated via admin interface";
                _context.BillingSettings.Update(settings);
            }

            await _context.SaveChangesAsync();
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing settings");
            throw;
        }
    }
}