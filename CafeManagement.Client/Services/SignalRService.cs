using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CafeManagement.Client.Services;

public class SignalRService : ISignalRService, IDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILockScreenService _lockScreenService;
    private bool _isConnected = false;

    public SignalRService(ILogger<SignalRService> logger, IConfiguration configuration, ILockScreenService lockScreenService)
    {
        _logger = logger;
        _configuration = configuration;
        _lockScreenService = lockScreenService;
        _httpClient = new HttpClient();

        var serverUrl = _configuration["ServerSettings:BaseUrl"] ?? "http://localhost:5032";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hub/cafemanagement")
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _hubConnection.Reconnected += async (connectionId) =>
        {
            _isConnected = true;
            _logger.LogInformation($"SignalR reconnected with connection ID: {connectionId}");
            await OnReconnected();
        };

        _hubConnection.Reconnecting += (exception) =>
        {
            _isConnected = false;
            _logger.LogWarning($"SignalR reconnecting: {exception?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (exception) =>
        {
            _isConnected = false;
            _logger.LogWarning($"SignalR connection closed: {exception?.Message}");
            return Task.CompletedTask;
        };

        // Handle incoming server commands
        _hubConnection.On<byte[]>("ReceiveRemoteCommand", HandleRemoteCommand);
        _hubConnection.On<string>("ReceiveTextMessage", HandleTextMessage);
        _hubConnection.On<bool>("SetLockScreenState", SetLockScreenState);
        _hubConnection.On<int>("SetTimeWarning", SetTimeWarning);
        _hubConnection.On("UnlockWorkstation", UnlockWorkstation);
        _hubConnection.On("LockWorkstation", LockWorkstation);
        _hubConnection.On("StartRemoteControl", StartRemoteControl);
        _hubConnection.On("StopRemoteControl", StopRemoteControl);
    }

    public async Task ConnectAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("SignalR connection already established");
                return;
            }

            _logger.LogInformation("Connecting to SignalR hub...");
            await _hubConnection.StartAsync();
            _isConnected = true;
            _logger.LogInformation("SignalR connection established successfully");

            // Register client with server
            var clientId = await GetClientId();
            if (clientId > 0)
            {
                await _hubConnection.InvokeAsync("RegisterClient", clientId);
                _logger.LogInformation($"Client {clientId} registered with server");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to SignalR hub");
            _isConnected = false;
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                _logger.LogInformation("SignalR connection stopped");
            }
            _isConnected = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SignalR connection");
        }
    }

    public async Task NotifyUnlockRequested()
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("RequestUnlock");
                _logger.LogInformation("Unlock request sent to server");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending unlock request");
        }
    }

    public async Task NotifySessionStarted(SessionDto session)
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("SessionStarted", session);
                _logger.LogInformation($"Session started notification sent: {session.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending session started notification");
        }
    }

    public async Task NotifySessionEnded(SessionDto session)
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("SessionEnded", session);
                _logger.LogInformation($"Session ended notification sent: {session.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending session ended notification");
        }
    }

    public async Task NotifyClientStatusUpdated(int clientId, int status)
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("ClientStatusUpdated", clientId, status);
                _logger.LogInformation($"Client status updated notification sent: Client {clientId}, Status {status}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending client status updated notification");
        }
    }

    public async Task SendScreenshot(byte[] imageData)
    {
        try
        {
            if (_isConnected && imageData.Length > 0)
            {
                await _hubConnection.InvokeAsync("ReceiveScreenshot", imageData);
                _logger.LogDebug($"Screenshot sent: {imageData.Length} bytes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending screenshot");
        }
    }

    public async Task NotifyRemoteControlStarted()
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("RemoteControlStarted");
                _logger.LogInformation("Remote control started notification sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending remote control started notification");
        }
    }

    public async Task NotifyRemoteControlStopped()
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("RemoteControlStopped");
                _logger.LogInformation("Remote control stopped notification sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending remote control stopped notification");
        }
    }

    public async Task SendRemoteCommand(string command, params object[] parameters)
    {
        try
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("RemoteCommand", command, parameters);
                _logger.LogDebug($"Remote command sent: {command}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending remote command: {command}");
        }
    }

    private async Task<int> GetClientId()
    {
        try
        {
            // Get client information
            var clientInfo = new
            {
                Name = Environment.MachineName,
                IpAddress = GetLocalIpAddress(),
                MacAddress = GetMacAddress()
            };

            var content = JsonContent.Create(clientInfo);
            var serverUrl = _configuration["ServerSettings:BaseUrl"] ?? "http://localhost:5032";
            var response = await _httpClient.PostAsync($"{serverUrl}/api/clients", content);

            if (response.IsSuccessStatusCode)
            {
                var clientDto = await response.Content.ReadFromJsonAsync<ClientDto>();
                return clientDto?.Id ?? 0;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client ID");
            return 0;
        }
    }

    private string GetLocalIpAddress()
    {
        try
        {
            using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    private string GetMacAddress()
    {
        try
        {
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var firstNic = nics.FirstOrDefault(nic => nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);
            return firstNic?.GetPhysicalAddress().ToString() ?? "00-00-00-00-00-00";
        }
        catch
        {
            return "00-00-00-00-00-00";
        }
    }

    // Event handlers for server commands
    private Task HandleRemoteCommand(byte[] commandData)
    {
        try
        {
            var command = System.Text.Encoding.UTF8.GetString(commandData);
            _logger.LogInformation($"Remote command received: {command}");

            // Parse and execute remote command
            // This would integrate with the RemoteControlService
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling remote command");
            return Task.CompletedTask;
        }
    }

    private Task HandleTextMessage(string message)
    {
        _logger.LogInformation($"Text message received: {message}");
        return Task.CompletedTask;
    }

    private Task SetLockScreenState(bool isLocked)
    {
        _logger.LogInformation($"Lock screen state set to: {isLocked}");
        return Task.CompletedTask;
    }

    private Task SetTimeWarning(int minutes)
    {
        _logger.LogInformation($"Time warning: {minutes} minutes remaining");
        return Task.CompletedTask;
    }

    private Task UnlockWorkstation()
    {
        try
        {
            _logger.LogInformation("Workstation unlock command received");
            _lockScreenService.HideLockScreen();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling workstation unlock command");
            return Task.CompletedTask;
        }
    }

    private Task LockWorkstation()
    {
        try
        {
            _logger.LogInformation("Workstation lock command received");
            _lockScreenService.ShowLockScreen();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling workstation lock command");
            return Task.CompletedTask;
        }
    }

    private Task StartRemoteControl()
    {
        _logger.LogInformation("Remote control start command received");
        return Task.CompletedTask;
    }

    private Task StopRemoteControl()
    {
        _logger.LogInformation("Remote control stop command received");
        return Task.CompletedTask;
    }

    private async Task OnReconnected()
    {
        // Re-register client after reconnection
        var clientId = await GetClientId();
        if (clientId > 0)
        {
            await _hubConnection.InvokeAsync("RegisterClient", clientId);
        }
    }

    public void Dispose()
    {
        _hubConnection?.DisposeAsync().AsTask().Wait();
        _httpClient?.Dispose();
    }
}