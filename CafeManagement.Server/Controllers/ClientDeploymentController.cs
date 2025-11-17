using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeManagement.Application.Services;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Operator")]
public class ClientDeploymentController : ControllerBase
{
    private readonly IDeploymentService _deploymentService;
    private readonly ILogger<ClientDeploymentController> _logger;

    public ClientDeploymentController(IDeploymentService deploymentService, ILogger<ClientDeploymentController> logger)
    {
        _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDeployment>>> GetAllDeployments()
    {
        try
        {
            var deployments = await _deploymentService.GetAllDeploymentsAsync();
            return Ok(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployments");
            return StatusCode(500, "Internal server error while retrieving deployments");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDeployment>> GetDeployment(int id)
    {
        try
        {
            var deployment = await _deploymentService.GetDeploymentByIdAsync(id);
            if (deployment == null)
            {
                return NotFound($"Deployment with ID {id} not found");
            }
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment {Id}", id);
            return StatusCode(500, $"Internal server error while retrieving deployment {id}");
        }
    }

    [HttpGet("by-ip/{ipAddress}")]
    public async Task<ActionResult<ClientDeployment>> GetDeploymentByIp(string ipAddress)
    {
        try
        {
            var deployment = await _deploymentService.GetDeploymentByIpAsync(ipAddress);
            if (deployment == null)
            {
                return NotFound($"Deployment with IP {ipAddress} not found");
            }
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment by IP {IpAddress}", ipAddress);
            return StatusCode(500, "Internal server error while retrieving deployment by IP");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ClientDeployment>> CreateDeployment([FromBody] CreateDeploymentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ClientName) ||
                string.IsNullOrWhiteSpace(request.IpAddress))
            {
                return BadRequest("Client name and IP address are required");
            }

            var deployment = new ClientDeployment
            {
                ClientName = request.ClientName,
                IpAddress = request.IpAddress,
                MacAddress = request.MacAddress,
                Location = request.Location,
                Version = request.Version ?? "1.0.0",
                TargetVersion = request.TargetVersion ?? "1.0.0",
                AutoUpdateEnabled = request.AutoUpdateEnabled,
                Status = DeploymentStatus.Pending
            };

            var createdDeployment = await _deploymentService.CreateDeploymentAsync(deployment);
            return CreatedAtAction(nameof(GetDeployment), new { id = createdDeployment.Id }, createdDeployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deployment");
            return StatusCode(500, "Internal server error while creating deployment");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDeployment>> UpdateDeployment(int id, [FromBody] UpdateDeploymentRequest request)
    {
        try
        {
            var existingDeployment = await _deploymentService.GetDeploymentByIdAsync(id);
            if (existingDeployment == null)
            {
                return NotFound($"Deployment with ID {id} not found");
            }

            if (!string.IsNullOrWhiteSpace(request.ClientName))
                existingDeployment.ClientName = request.ClientName;

            if (!string.IsNullOrWhiteSpace(request.IpAddress))
                existingDeployment.IpAddress = request.IpAddress;

            if (!string.IsNullOrWhiteSpace(request.MacAddress))
                existingDeployment.MacAddress = request.MacAddress;

            if (!string.IsNullOrWhiteSpace(request.Location))
                existingDeployment.Location = request.Location;

            if (!string.IsNullOrWhiteSpace(request.Version))
                existingDeployment.Version = request.Version;

            if (!string.IsNullOrWhiteSpace(request.TargetVersion))
                existingDeployment.TargetVersion = request.TargetVersion;

            existingDeployment.AutoUpdateEnabled = request.AutoUpdateEnabled;

            var updatedDeployment = await _deploymentService.UpdateDeploymentAsync(existingDeployment);
            return Ok(updatedDeployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deployment {Id}", id);
            return StatusCode(500, $"Internal server error while updating deployment {id}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDeployment(int id)
    {
        try
        {
            var success = await _deploymentService.DeleteDeploymentAsync(id);
            if (!success)
            {
                return NotFound($"Deployment with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deployment {Id}", id);
            return StatusCode(500, $"Internal server error while deleting deployment {id}");
        }
    }

    [HttpPost("{id}/deploy")]
    public async Task<ActionResult> DeployToClient(int id, [FromBody] DeployRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TargetVersion))
            {
                return BadRequest("Target version is required");
            }

            var performedBy = User.Identity?.Name ?? "Unknown";
            var success = await _deploymentService.DeployToClientAsync(id, request.TargetVersion, performedBy);

            if (!success)
            {
                return BadRequest("Deployment failed");
            }

            return Ok(new { message = "Deployment started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying to client {Id}", id);
            return StatusCode(500, $"Internal server error while deploying to client {id}");
        }
    }

    [HttpPost("{id}/check-status")]
    public async Task<ActionResult> CheckClientStatus(int id)
    {
        try
        {
            var isOnline = await _deploymentService.CheckClientStatusAsync(id);
            return Ok(new { isOnline, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking client status {Id}", id);
            return StatusCode(500, $"Internal server error while checking client status {id}");
        }
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<DeploymentLog>>> GetDeploymentLogs(int id, [FromQuery] int limit = 50)
    {
        try
        {
            var logs = await _deploymentService.GetDeploymentLogsAsync(id, limit);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment logs {Id}", id);
            return StatusCode(500, $"Internal server error while retrieving deployment logs {id}");
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ClientDeployment>> RegisterClient([FromBody] RegisterClientRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ClientName) ||
                string.IsNullOrWhiteSpace(request.IpAddress))
            {
                return BadRequest("Client name and IP address are required");
            }

            var deployment = await _deploymentService.RegisterClientAsync(
                request.ClientName,
                request.IpAddress,
                request.MacAddress ?? "",
                request.Location ?? "");

            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering client");
            return StatusCode(500, "Internal server error while registering client");
        }
    }

    [HttpGet("offline")]
    public async Task<ActionResult<IEnumerable<ClientDeployment>>> GetOfflineClients([FromQuery] int thresholdMinutes = 5)
    {
        try
        {
            var threshold = TimeSpan.FromMinutes(thresholdMinutes);
            var offlineClients = await _deploymentService.GetOfflineDeploymentsAsync(threshold);
            return Ok(offlineClients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving offline clients");
            return StatusCode(500, "Internal server error while retrieving offline clients");
        }
    }

    [HttpPut("{id}/version")]
    public async Task<ActionResult> UpdateClientVersion(int id, [FromBody] UpdateVersionRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Version))
            {
                return BadRequest("Version is required");
            }

            var success = await _deploymentService.UpdateClientVersionAsync(id, request.Version);
            if (!success)
            {
                return NotFound($"Deployment with ID {id} not found");
            }

            return Ok(new { message = "Version updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client version {Id}", id);
            return StatusCode(500, $"Internal server error while updating client version {id}");
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<DeploymentStatistics>> GetDeploymentStatistics()
    {
        try
        {
            var statistics = await _deploymentService.GetDeploymentStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment statistics");
            return StatusCode(500, "Internal server error while retrieving deployment statistics");
        }
    }
}

public class CreateDeploymentRequest
{
    public string ClientName { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? MacAddress { get; set; }
    public string? Location { get; set; }
    public string? Version { get; set; }
    public string? TargetVersion { get; set; }
    public bool AutoUpdateEnabled { get; set; } = true;
}

public class UpdateDeploymentRequest
{
    public string? ClientName { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public string? Location { get; set; }
    public string? Version { get; set; }
    public string? TargetVersion { get; set; }
    public bool AutoUpdateEnabled { get; set; }
}

public class DeployRequest
{
    public string TargetVersion { get; set; } = "";
}

public class RegisterClientRequest
{
    public string ClientName { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? MacAddress { get; set; }
    public string? Location { get; set; }
    public string? Version { get; set; } = "1.0.0";
}

public class UpdateVersionRequest
{
    public string Version { get; set; } = "";
}