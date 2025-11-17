using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeManagement.Application.Services;
using CafeManagement.Core.Entities;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Operator")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<BillingSettings>> GetCurrentSettings()
    {
        try
        {
            var settings = await _billingService.GetCurrentSettingsAsync();
            if (settings == null)
            {
                return NotFound("No billing settings found. Please configure billing settings.");
            }
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing settings");
            return StatusCode(500, "Internal server error while retrieving billing settings");
        }
    }

    [HttpGet("rate")]
    public async Task<ActionResult<decimal>> GetHourlyRate()
    {
        try
        {
            var rate = await _billingService.GetHourlyRateAsync();
            return Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hourly rate");
            return StatusCode(500, "Internal server error while retrieving hourly rate");
        }
    }

    [HttpPut("settings")]
    public async Task<ActionResult<BillingSettings>> UpdateSettings([FromBody] UpdateBillingSettingsRequest request)
    {
        try
        {
            if (request == null || request.HourlyRate <= 0)
            {
                return BadRequest("Hourly rate must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                return BadRequest("Currency is required");
            }

            var settings = await _billingService.UpdateSettingsAsync(request.HourlyRate, request.Currency);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing settings");
            return StatusCode(500, "Internal server error while updating billing settings");
        }
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<decimal>> CalculateSessionCost([FromBody] CalculateSessionCostRequest request)
    {
        try
        {
            if (request.DurationMinutes <= 0)
            {
                return BadRequest("Duration must be greater than 0");
            }

            var cost = await _billingService.CalculateSessionCostAsync(request.DurationMinutes);
            return Ok(cost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating session cost");
            return StatusCode(500, "Internal server error while calculating session cost");
        }
    }
}

public class UpdateBillingSettingsRequest
{
    public decimal HourlyRate { get; set; }
    public string Currency { get; set; } = "L.E";
}

public class CalculateSessionCostRequest
{
    public int DurationMinutes { get; set; }
}