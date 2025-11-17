using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CafeManagement.Server.Hubs;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Application.DTOs;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IHubContext<CafeManagementHub> _hubContext;
    private readonly ILogger<AdminController> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AdminController(
        IHubContext<CafeManagementHub> hubContext,
        ILogger<AdminController> logger,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [HttpGet("clients")]
    public async Task<ActionResult<List<ClientDto>>> GetClients()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            // Get all online clients
            var clients = await clientService.GetOnlineClientsAsync();
            var clientDtos = clients.Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                IPAddress = c.IPAddress,
                MACAddress = c.MACAddress,
                Status = c.Status,
                LastSeen = c.LastSeen,
                CurrentSessionId = c.CurrentSessionId
            }).ToList();

            return Ok(clientDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clients");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("clients/{clientId}")]
    public async Task<ActionResult<ClientDto>> GetClient(int clientId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var client = await clientService.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return NotFound($"Client with ID {clientId} not found");
            }

            var clientDto = new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                IPAddress = client.IPAddress,
                MACAddress = client.MACAddress,
                Status = client.Status,
                LastSeen = client.LastSeen,
                CurrentSessionId = client.CurrentSessionId
            };

            // Add online status
            clientDto.IsOnline = client.Status >= ClientStatus.Online && client.Status <= ClientStatus.InSession;

            return Ok(clientDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client {ClientId}", clientId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/start")]
    public async Task<ActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

            var session = await sessionService.StartSessionAsync(request.ClientId, request.UserId, request.DurationMinutes);

            // Notify all clients about the new session
            await _hubContext.Clients.All.SendAsync("LockWorkstation");

            _logger.LogInformation($"Session started for client {request.ClientId}");

            return Ok(new { message = $"Session started for client {request.ClientId}", sessionId = session.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting session");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/{sessionId}/end")]
    public async Task<ActionResult> EndSession(int sessionId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

            var session = await sessionService.EndSessionAsync(sessionId);

            // Unlock the client
            await _hubContext.Clients.All.SendAsync("UnlockWorkstation");

            _logger.LogInformation($"Session {sessionId} ended");

            return Ok(new { message = $"Session {sessionId} ended", totalCost = session.TotalAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/{sessionId}/extend")]
    public async Task<ActionResult> ExtendSession(int sessionId, [FromBody] ExtendSessionRequest request)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

            var session = await sessionService.ExtendSessionAsync(sessionId, request.AdditionalMinutes);

            _logger.LogInformation($"Session {sessionId} extended by {request.AdditionalMinutes} minutes");

            return Ok(new { message = $"Session {sessionId} extended", newDuration = session.DurationMinutes, newCost = session.TotalAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/lock")]
    public async Task<ActionResult> LockClient(int clientId)
    {
        try
        {
            _logger.LogInformation($"Locking client {clientId}");

            // Send lock command to all clients
            await _hubContext.Clients.All.SendAsync("LockWorkstation");

            return Ok(new { message = $"Client {clientId} locked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error locking client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/unlock")]
    public async Task<ActionResult> UnlockClient(int clientId)
    {
        try
        {
            _logger.LogInformation($"Unlocking client {clientId}");

            // Send unlock command to all clients via SignalR
            await _hubContext.Clients.All.SendAsync("UnlockWorkstation");

            // Also set force unlock flag as fallback
            _unlockCommands[clientId] = true;

            return Ok(new { message = $"Client {clientId} unlocked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unlocking client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("test-lock")]
    public async Task<ActionResult> TestLockScreen()
    {
        try
        {
            _logger.LogInformation("Testing lock screen functionality");
            await _hubContext.Clients.All.SendAsync("LockWorkstation");
            return Ok(new { message = "Lock screen test command sent to all clients" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing lock screen");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<object>>> GetSessions()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var sessions = await sessionService.GetAllSessionsAsync();
            var result = new List<object>();

            foreach (var session in sessions)
            {
                var client = await clientService.GetClientByIdAsync(session.ClientId);
                var elapsedMinutes = (DateTime.UtcNow - session.StartTime).TotalMinutes;
                var remainingMinutes = Math.Max(0, session.DurationMinutes - (int)elapsedMinutes);
                var status = remainingMinutes > 0 ? "Active" : "Completed";

                result.Add(new
                {
                    id = session.Id,
                    clientId = session.ClientId,
                    clientName = client?.Name ?? "Unknown",
                    startTime = session.StartTime,
                    durationMinutes = session.DurationMinutes,
                    remainingMinutes = remainingMinutes,
                    totalCost = session.TotalAmount,
                    status = status
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("sessions/active")]
    public async Task<ActionResult<List<object>>> GetActiveSessions()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var activeSessions = await sessionService.GetActiveSessionsAsync();
            var result = new List<object>();

            foreach (var session in activeSessions)
            {
                var client = await clientService.GetClientByIdAsync(session.ClientId);
                var elapsedMinutes = (DateTime.UtcNow - session.StartTime).TotalMinutes;
                var remainingMinutes = Math.Max(0, session.DurationMinutes - (int)elapsedMinutes);

                result.Add(new
                {
                    id = session.Id,
                    clientId = session.ClientId,
                    clientName = client?.Name ?? "Unknown",
                    startTime = session.StartTime,
                    durationMinutes = session.DurationMinutes,
                    remainingMinutes = remainingMinutes,
                    totalCost = session.TotalAmount
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions");
            return StatusCode(500, "Internal server error");
        }
    }

    private static readonly Dictionary<int, bool> _unlockCommands = new();

    [HttpGet("clients/{clientId}/should-unlock")]
    public ActionResult<bool> ShouldUnlock(int clientId)
    {
        try
        {
            var shouldUnlock = _unlockCommands.GetValueOrDefault(clientId, false);
            if (shouldUnlock)
            {
                _unlockCommands[clientId] = false; // Clear the command after reading
                _logger.LogInformation($"Client {clientId} unlock command retrieved");
            }
            return Ok(shouldUnlock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking unlock status for client {clientId}");
            return StatusCode(500, false);
        }
    }

    [HttpPost("clients/{clientId}/unlock-force")]
    public ActionResult ForceUnlock(int clientId)
    {
        try
        {
            _unlockCommands[clientId] = true;
            _logger.LogInformation($"Force unlock command set for client {clientId}");
            return Ok(new { message = $"Force unlock command sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting force unlock for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    
    [HttpPost("clients/{clientId}/simulate-mouse")]
    public async Task<ActionResult> SimulateMouse(int clientId, [FromBody] MouseInputRequest request)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("SimulateMouse", request);
            return Ok(new { message = $"Mouse command sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error simulating mouse for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/simulate-keyboard")]
    public async Task<ActionResult> SimulateKeyboard(int clientId, [FromBody] KeyboardInputRequest request)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("SimulateKeyboard", request);
            return Ok(new { message = $"Keyboard command sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error simulating keyboard for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/simulate-text")]
    public async Task<ActionResult> SimulateText(int clientId, [FromBody] TextInputRequest request)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("SimulateText", request);
            return Ok(new { message = $"Text command sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error simulating text for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    // New remote monitoring endpoints
    [HttpPost("clients/{clientId}/screenshot")]
    public async Task<ActionResult> CaptureScreenshot(int clientId)
    {
        try
        {
            _logger.LogInformation($"Attempting to capture screenshot from client {clientId}");

            // Request screenshot from client
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("CaptureScreenshot");
            _logger.LogInformation($"SignalR 'CaptureScreenshot' message sent to group Client_{clientId}");

            // Store screenshot request for retrieval
            _screenshotRequests[clientId] = DateTime.UtcNow;

            return Ok(new { message = $"Screenshot request sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error requesting screenshot from client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/start-remote-control")]
    public async Task<ActionResult> StartRemoteControlSession(int clientId)
    {
        try
        {
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("StartRemoteControl");
            return Ok(new { message = $"Remote control started for client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting remote control for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/stop-remote-control")]
    public async Task<ActionResult> StopRemoteControlSession(int clientId)
    {
        try
        {
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("StopRemoteControl");
            return Ok(new { message = $"Remote control stopped for client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping remote control for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("clients/{clientId}/system-info")]
    public async Task<ActionResult> GetSystemInfo(int clientId)
    {
        try
        {
            // Get client info from database
            using var scope = _serviceProvider.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            var client = await clientService.GetClientByIdAsync(clientId);

            if (client == null)
                return NotFound(new { error = "Client not found" });

            // Request current system info from client
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("GetSystemInfo");

            // Return basic info immediately
            var systemInfo = new
            {
                computerName = Environment.MachineName,
                osVersion = Environment.OSVersion.ToString(),
                processor = Environment.ProcessorCount + " cores",
                totalMemory = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB",
                availableMemory = $"{GC.GetTotalMemory(true) / 1024 / 1024} MB",
                ipAddress = client.IPAddress,
                macAddress = client.MACAddress,
                userName = Environment.UserName,
                lastUpdated = DateTime.UtcNow
            };

            return Ok(systemInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting system info for client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/mouse-click")]
    public async Task<ActionResult> SendMouseClick(int clientId, [FromBody] MouseInputRequest request)
    {
        try
        {
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("SimulateMouse", request);
            return Ok(new { message = $"Mouse click sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending mouse click to client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/keyboard-input")]
    public async Task<ActionResult> SendKeyboardInput(int clientId, [FromBody] KeyboardInputRequest request)
    {
        try
        {
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("SimulateKeyboard", request);
            return Ok(new { message = $"Keyboard input sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending keyboard input to client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("clients/{clientId}/text-input")]
    public async Task<ActionResult> SendTextInput(int clientId, [FromBody] TextInputRequest request)
    {
        try
        {
            await _hubContext.Clients.Group($"Client_{clientId}").SendAsync("SimulateText", request);
            return Ok(new { message = $"Text input sent to client {clientId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending text input to client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    // SignalR Hub method for receiving screenshots
    [HttpPost("screenshot/{clientId}")]
    public async Task<ActionResult> ReceiveScreenshot(int clientId, [FromBody] ScreenshotDataRequest request)
    {
        try
        {
            // Store screenshot for immediate retrieval
            _latestScreenshots[clientId] = request.ScreenshotData;

            _logger.LogInformation($"Screenshot received from client {clientId}");
            return Ok(new { message = "Screenshot received successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error receiving screenshot from client {clientId}");
            return StatusCode(500, "Internal server error");
        }
    }

    // Private fields for storing data
    private static readonly Dictionary<int, DateTime> _screenshotRequests = new();
    private static readonly Dictionary<int, string> _latestScreenshots = new();
}

public class StartSessionRequest
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public int DurationMinutes { get; set; }
}

public class ExtendSessionRequest
{
    public int AdditionalMinutes { get; set; }
}

public class MouseInputRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Action { get; set; } = "click"; // "click", "move", "right-click", etc.
}

public class KeyboardInputRequest
{
    public int KeyCode { get; set; }
    public string Action { get; set; } = "keydown"; // "keydown", "keyup"
}

public class TextInputRequest
{
    public string Text { get; set; } = string.Empty;
}

public class ScreenshotDataRequest
{
    public string ScreenshotData { get; set; } = string.Empty; // Base64 encoded image
}