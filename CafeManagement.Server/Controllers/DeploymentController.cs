using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text.Json;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeploymentController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DeploymentController> _logger;

    public DeploymentController(IWebHostEnvironment environment, ILogger<DeploymentController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("check-updates")]
    public async Task<ActionResult<UpdateInfo>> CheckForUpdates(string currentVersion = "1.0.0.0")
    {
        try
        {
            var latestVersion = "1.1.0.0"; // This should come from a database or config file
            var isUpdateAvailable = CompareVersions(currentVersion, latestVersion) < 0;

            var updateInfo = new UpdateInfo
            {
                IsUpdateAvailable = isUpdateAvailable,
                LatestVersion = latestVersion,
                UpdateUrl = isUpdateAvailable ? "/api/deployment/download" : "",
                UpdateNotes = new List<string>
                {
                    "Bug fixes and performance improvements",
                    "Enhanced screen sharing capabilities",
                    "New remote control features"
                },
                ReleaseDate = DateTime.UtcNow.AddDays(-7),
                UpdateSize = isUpdateAvailable ? 15456789 : 0 // ~15MB
            };

            return Ok(updateInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates");
            return StatusCode(500, new { message = "Error checking for updates" });
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadUpdate()
    {
        try
        {
            var updatePath = Path.Combine(_environment.ContentRootPath, "Updates", "CafeManagementClient-1.1.0.0.exe");

            if (!System.IO.File.Exists(updatePath))
            {
                // Generate a mock update package for demonstration
                await CreateMockUpdatePackage(updatePath);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(updatePath);
            var fileName = $"CafeManagementClient-1.1.0.0.exe";

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading update");
            return StatusCode(500, new { message = "Error downloading update" });
        }
    }

    [HttpGet("installer")]
    public async Task<IActionResult> DownloadInstaller()
    {
        try
        {
            var installerPath = Path.Combine(_environment.ContentRootPath, "Installer", "CafeManagementClient-Setup.exe");

            if (!System.IO.File.Exists(installerPath))
            {
                // Generate a mock installer for demonstration
                await CreateMockInstaller(installerPath);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(installerPath);
            var fileName = "CafeManagementClient-Setup.exe";

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading installer");
            return StatusCode(500, new { message = "Error downloading installer" });
        }
    }

    [HttpPost("register-client")]
    public async Task<ActionResult<ClientDto>> RegisterClient([FromBody] ClientRegistrationDto registrationDto)
    {
        try
        {
            // In a real implementation, this would register the client in the database
            // For now, return a mock response
            var clientDto = new ClientDto
            {
                Id = new Random().Next(1000, 9999),
                Name = registrationDto.Name,
                IPAddress = registrationDto.IpAddress,
                MACAddress = registrationDto.MacAddress,
                Status = Core.Enums.ClientStatus.Online,
                Configuration = "{}",
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(clientDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering client");
            return StatusCode(500, new { message = "Error registering client" });
        }
    }

    [HttpGet("client-config/{clientId}")]
    public async Task<ActionResult> GetClientConfig(int clientId)
    {
        try
        {
            // Generate client configuration
            var config = new
            {
                ServerUrl = Request.Scheme + "://" + Request.Host,
                ClientId = clientId.ToString(),
                LockScreenSettings = new
                {
                    BackgroundColor = "#000033",
                    TextColor = "#FFFFFF",
                    ShowTimeRemaining = true,
                    CustomMessage = "Welcome to Cafe Management System"
                },
                RemoteControlSettings = new
                {
                    Enabled = true,
                    RequireConfirmation = true,
                    Quality = "High",
                    FrameRate = 10
                },
                AutoUpdateSettings = new
                {
                    Enabled = true,
                    CheckInterval = 3600, // 1 hour
                    AutoInstall = false
                }
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting client config for client {clientId}");
            return StatusCode(500, new { message = "Error getting client configuration" });
        }
    }

    [HttpPost("deploy/{clientId}")]
    public async Task<ActionResult> DeployToClient(int clientId, [FromBody] DeploymentRequestDto deploymentDto)
    {
        try
        {
            _logger.LogInformation($"Deployment requested for client {clientId}: {deploymentDto.DeploymentType}");

            // SignalR notification would be sent to specific client
            // For now, return success

            var response = new
            {
                Success = true,
                Message = "Deployment initiated successfully",
                DeploymentId = Guid.NewGuid().ToString()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deploying to client {clientId}");
            return StatusCode(500, new { message = "Error deploying to client" });
        }
    }

    private int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
        var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

        for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
        {
            var v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            var v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

            if (v1Part != v2Part)
            {
                return v1Part.CompareTo(v2Part);
            }
        }

        return 0;
    }

    private async Task CreateMockUpdatePackage(string updatePath)
    {
        var updateDir = Path.GetDirectoryName(updatePath);
        Directory.CreateDirectory(updateDir, true);

        // Create a mock executable (this would be the actual updated client)
        await System.IO.File.WriteAllBytesAsync(updatePath, new byte[1024]); // 1KB mock file
    }

    private async Task CreateMockInstaller(string installerPath)
    {
        var installerDir = Path.GetDirectoryName(installerPath);
        Directory.CreateDirectory(installerDir, true);

        // Create a mock installer (this would be the actual installer)
        await System.IO.File.WriteAllBytesAsync(installerPath, new byte[2048]); // 2KB mock file
    }
}

public class ClientRegistrationDto
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string MacAddress { get; set; } = "";
}

public class DeploymentRequestDto
{
    public string DeploymentType { get; set; } = "Update"; // Update, Install, Configure
    public string Version { get; set; } = "1.1.0.0";
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class UpdateInfo
{
    public bool IsUpdateAvailable { get; set; }
    public string LatestVersion { get; set; } = "";
    public string UpdateUrl { get; set; } = "";
    public List<string> UpdateNotes { get; set; } = new();
    public DateTime ReleaseDate { get; set; }
    public long UpdateSize { get; set; }
}