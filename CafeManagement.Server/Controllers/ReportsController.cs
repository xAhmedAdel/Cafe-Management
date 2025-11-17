using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Services;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("system-overview")]
    public async Task<ActionResult<SystemOverviewDto>> GetSystemOverview()
    {
        try
        {
            var overview = await _reportService.GetSystemOverviewAsync();
            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system overview");
            return StatusCode(500, "An error occurred while retrieving system overview");
        }
    }

    [HttpGet("user-usage/{userId}")]
    public async Task<ActionResult<UserUsageReportDto>> GetUserUsageReport(
        int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var report = await _reportService.GetUserUsageReportAsync(userId, startDate, endDate);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user usage report for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user usage report");
        }
    }

    [HttpGet("user-usage")]
    public async Task<ActionResult<List<UserUsageReportDto>>> GetAllUsersUsageReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var reports = await _reportService.GetAllUsersUsageReportAsync(startDate, endDate);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users usage report");
            return StatusCode(500, "An error occurred while retrieving users usage report");
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var report = await _reportService.GetRevenueReportAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue report for period {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving revenue report");
        }
    }

    [HttpGet("client-usage/{clientId}")]
    public async Task<ActionResult<ClientUsageReportDto>> GetClientUsageReport(
        int clientId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var report = await _reportService.GetClientUsageReportAsync(clientId, startDate, endDate);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client usage report for client {ClientId}", clientId);
            return StatusCode(500, "An error occurred while retrieving client usage report");
        }
    }

    [HttpGet("client-usage")]
    public async Task<ActionResult<List<ClientUsageReportDto>>> GetAllClientsUsageReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var reports = await _reportService.GetAllClientsUsageReportAsync(startDate, endDate);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all clients usage report");
            return StatusCode(500, "An error occurred while retrieving clients usage report");
        }
    }

    [HttpPost("export/user-usage")]
    public async Task<ActionResult<ExportReportDto>> ExportUserUsageReport(
        [FromBody] ReportParametersDto parameters)
    {
        try
        {
            if (parameters.StartDate >= parameters.EndDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var exportReport = await _reportService.ExportUserUsageReportAsync(
                parameters.StartDate,
                parameters.EndDate,
                parameters.Format);

            if (string.IsNullOrEmpty(exportReport.FileName))
            {
                return BadRequest("Failed to generate export report");
            }

            return File(
                exportReport.Data,
                exportReport.MimeType,
                exportReport.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user usage report");
            return StatusCode(500, "An error occurred while exporting user usage report");
        }
    }

    [HttpPost("export/revenue")]
    public async Task<ActionResult<ExportReportDto>> ExportRevenueReport(
        [FromBody] ReportParametersDto parameters)
    {
        try
        {
            if (parameters.StartDate >= parameters.EndDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var exportReport = await _reportService.ExportRevenueReportAsync(
                parameters.StartDate,
                parameters.EndDate,
                parameters.Format);

            if (string.IsNullOrEmpty(exportReport.FileName))
            {
                return BadRequest("Failed to generate export report");
            }

            return File(
                exportReport.Data,
                exportReport.MimeType,
                exportReport.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting revenue report");
            return StatusCode(500, "An error occurred while exporting revenue report");
        }
    }
}