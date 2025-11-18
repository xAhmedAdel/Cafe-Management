using CafeManagement.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.Enums;

namespace CafeManagement.Server.Hubs;

public class ClientConnectionInfo
{
    public int ClientId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeen { get; set; }
    public string? IPAddress { get; set; }
    public bool IsActive => DateTime.UtcNow.Subtract(LastSeen).TotalMinutes < 2;
}

public interface ICafeManagementHubClient
{
    Task ClientStatusUpdated(ClientDto client);
    Task SessionStarted(SessionDto session);
    Task SessionEnded(SessionDto session);
    Task SessionExtended(SessionDto session);
    Task SystemMessage(string message);
    Task UserBalanceUpdated(UserDto user);
    Task ReceiveScreenshot(byte[] imageData);
    Task ReceiveRemoteCommand(byte[] commandData);
    Task ReceiveTextMessage(string message);
    Task SetLockScreenState(bool isLocked);
    Task SetTimeWarning(int minutes);
    Task UnlockWorkstation();
    Task LockWorkstation();
    Task StartRemoteControl();
    Task StopRemoteControl();
    Task ClientDisconnected(int clientId);
}

public class CafeManagementHub : Hub<ICafeManagementHubClient>
{
    private readonly ILogger<CafeManagementHub> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly Dictionary<string, ClientConnectionInfo> _connectionInfos = new();
    private static readonly Dictionary<int, List<ClientConnectionInfo>> _clientConnections = new();

    public CafeManagementHub(ILogger<CafeManagementHub> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"Client connected: {connectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"Client disconnected: {connectionId}");

        // Remove from enhanced tracking
        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            _connectionInfos.Remove(connectionId);

            if (_clientConnections.TryGetValue(clientId, out var connections))
            {
                var removedConnection = connections.FirstOrDefault(c => c.ConnectionId == connectionId);
                if (removedConnection != null)
                {
                    connections.Remove(removedConnection);
                }

                if (connections.Count == 0)
                {
                    _clientConnections.Remove(clientId);

                    // Update client status to offline in database
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
                        await clientService.UpdateClientStatusAsync(clientId, ClientStatus.Offline);

                        _logger.LogInformation($"Client {clientId} status updated to Offline");

                        // Get updated client data for notification
                        var client = await clientService.GetClientByIdAsync(clientId);
                        if (client != null)
                        {
                            var clientDto = new ClientDto
                            {
                                Id = client.Id,
                                Name = client.Name,
                                IPAddress = client.IPAddress,
                                MACAddress = client.MACAddress,
                                Status = client.Status,
                                LastSeen = client.LastSeen,
                                CurrentSessionId = client.CurrentSessionId,
                                IsOnline = false,
                                ConnectionDetails = new ConnectionDetailsDto
                                {
                                    ConnectionId = connectionId,
                                    ConnectedAt = removedConnection?.ConnectedAt ?? DateTime.UtcNow,
                                    LastSeen = DateTime.UtcNow,
                                    IsActive = false,
                                    IPAddress = removedConnection?.IPAddress
                                }
                            };

                            // Notify all operators/admins about the status change
                            await Clients.Group("Operators").ClientStatusUpdated(clientDto);
                            await Clients.Group("Administrators").ClientStatusUpdated(clientDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating client {clientId} status to offline");
                    }

                    // Notify other clients that this client is offline
                    await Clients.Others.ClientDisconnected(clientId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterClient(int clientId)
    {
        var connectionId = Context.ConnectionId;
        var httpContext = Context.GetHttpContext();
        var clientIP = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

        var connectionInfo = new ClientConnectionInfo
        {
            ClientId = clientId,
            ConnectionId = connectionId,
            ConnectedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            IPAddress = clientIP
        };

        _connectionInfos[connectionId] = connectionInfo;

        if (!_clientConnections.ContainsKey(clientId))
        {
            _clientConnections[clientId] = new List<ClientConnectionInfo>();
        }
        _clientConnections[clientId].Add(connectionInfo);

        // Add this connection to a group for the client
        await Groups.AddToGroupAsync(connectionId, $"Client_{clientId}");

        // Update client status to idle in database (initial state after login)
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            await clientService.UpdateClientStatusAsync(clientId, ClientStatus.Idle);

            _logger.LogInformation($"Client {clientId} status updated to Idle");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating client {clientId} status to online");
        }

        _logger.LogInformation($"Client {clientId} registered with connection {connectionId} from IP {clientIP}");
    }

    public async Task JoinOperatorGroup()
    {
        var connectionId = Context.ConnectionId;
        await Groups.AddToGroupAsync(connectionId, "Operators");
        _logger.LogInformation($"Operator connected: {connectionId}");
    }

    public async Task JoinAdminGroup()
    {
        var connectionId = Context.ConnectionId;
        await Groups.AddToGroupAsync(connectionId, "Administrators");
        await Groups.AddToGroupAsync(connectionId, "Operators"); // Admins are also operators
        _logger.LogInformation($"Administrator connected: {connectionId}");
    }

    // Server methods to broadcast updates
    public async Task NotifyClientStatusUpdate(ClientDto client)
    {
        await Clients.Group("Operators").ClientStatusUpdated(client);
        await Clients.Group($"Client_{client.Id}").ClientStatusUpdated(client);
    }

    public async Task NotifySessionStarted(SessionDto session)
    {
        await Clients.Group("Operators").SessionStarted(session);
        await Clients.Group($"Client_{session.ClientId}").SessionStarted(session);
    }

    public async Task NotifySessionEnded(SessionDto session)
    {
        await Clients.Group("Operators").SessionEnded(session);
        await Clients.Group($"Client_{session.ClientId}").SessionEnded(session);
    }

    public async Task NotifySessionExtended(SessionDto session)
    {
        await Clients.Group("Operators").SessionExtended(session);
        await Clients.Group($"Client_{session.ClientId}").SessionExtended(session);
    }

    public async Task NotifyUserBalanceUpdated(UserDto user)
    {
        await Clients.Group("Operators").UserBalanceUpdated(user);
        await Clients.Group($"Client_{user.Id}").UserBalanceUpdated(user);
    }

    public async Task BroadcastSystemMessage(string message, string targetGroup = "All")
    {
        if (targetGroup == "All")
        {
            await Clients.All.SystemMessage(message);
        }
        else if (targetGroup == "Operators")
        {
            await Clients.Group("Operators").SystemMessage(message);
        }
        else if (targetGroup == "Administrators")
        {
            await Clients.Group("Administrators").SystemMessage(message);
        }
    }

    public async Task NotifyClientDisconnected(int clientId)
    {
        await Clients.Group("Operators").SystemMessage($"Client {clientId} has disconnected");
    }

    public async Task<ClientStatusDto[]> GetConnectedClients()
    {
        // Return list of connected clients with their status using enhanced tracking
        var connectedClients = _clientConnections.Keys
            .Select(clientId => new ClientStatusDto
            {
                ClientId = clientId,
                IsConnected = true,
                ConnectionCount = _clientConnections[clientId].Count
            })
            .ToArray();

        return connectedClients;
    }

    public async Task UpdateLastSeen()
    {
        var connectionId = Context.ConnectionId;

        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            connectionInfo.LastSeen = DateTime.UtcNow;
            _logger.LogDebug($"Updated last seen for connection {connectionId} (client {connectionInfo.ClientId})");
        }
    }

    public Dictionary<int, List<ClientConnectionInfo>> GetAllConnectionInfos()
    {
        return _clientConnections;
    }

    public ClientConnectionInfo? GetConnectionInfo(int clientId)
    {
        if (_clientConnections.TryGetValue(clientId, out var connections))
        {
            return connections.FirstOrDefault(c => c.IsActive);
        }
        return null;
    }

    // Screen sharing methods
    public async Task ReceiveScreenshot(byte[] imageData)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"🖼️ ReceiveScreenshot called from connection {connectionId}, data length: {imageData.Length} bytes");

        // Find which client sent this screenshot using enhanced tracking
        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            connectionInfo.LastSeen = DateTime.UtcNow; // Update activity

            _logger.LogInformation($"📤 Broadcasting screenshot from client {clientId} ({imageData.Length} bytes) to Operators and Administrators groups");

            // Broadcast screenshot to all connected operators
            await Clients.Group("Operators").ReceiveScreenshot(imageData);
            await Clients.Group("Administrators").ReceiveScreenshot(imageData);

            _logger.LogInformation($"✅ Screenshot broadcast completed from client {clientId}");
        }
        else
        {
            _logger.LogWarning($"❌ Could not find client connection info for connection {connectionId}");
        }
    }

    public async Task RemoteControlStarted()
    {
        var connectionId = Context.ConnectionId;

        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            connectionInfo.LastSeen = DateTime.UtcNow; // Update activity

            // Notify all operators that remote control has started
            await Clients.Group("Operators").SystemMessage($"Remote control started for client {clientId}");
            await Clients.Group("Administrators").SystemMessage($"Remote control started for client {clientId}");

            _logger.LogInformation($"Remote control started for client {clientId}");
        }
    }

    public async Task RemoteControlStopped()
    {
        var connectionId = Context.ConnectionId;

        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            connectionInfo.LastSeen = DateTime.UtcNow; // Update activity

            // Notify all operators that remote control has stopped
            await Clients.Group("Operators").SystemMessage($"Remote control stopped for client {clientId}");
            await Clients.Group("Administrators").SystemMessage($"Remote control stopped for client {clientId}");

            _logger.LogInformation($"Remote control stopped for client {clientId}");
        }
    }

    public async Task RemoteCommand(string command, object[] parameters)
    {
        var connectionId = Context.ConnectionId;

        // Forward the remote command to the specified client
        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            connectionInfo.LastSeen = DateTime.UtcNow; // Update activity

            // Send command to specific client
            await Clients.Group($"Client_{clientId}").ReceiveRemoteCommand(System.Text.Encoding.UTF8.GetBytes($"{command}:{string.Join(",", parameters)}"));

            _logger.LogDebug($"Remote command forwarded to client {clientId}: {command}");
        }
    }

    // Client control methods
    public async Task RequestUnlock()
    {
        var connectionId = Context.ConnectionId;

        if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
        {
            var clientId = connectionInfo.ClientId;
            connectionInfo.LastSeen = DateTime.UtcNow; // Update activity

            // Notify operators that unlock is requested
            await Clients.Group("Operators").SystemMessage($"Client {clientId} is requesting unlock");

            _logger.LogInformation($"Unlock request received from client {clientId}");
        }
    }

    // Methods for sending commands to clients
    public async Task SendScreenshotRequestToClient(int clientId)
    {
        await Clients.Group($"Client_{clientId}").SystemMessage("Screenshot requested");
    }

    public async Task SendRemoteCommandToClient(int clientId, string command, params object[] parameters)
    {
        var commandData = System.Text.Encoding.UTF8.GetBytes($"{command}:{string.Join(",", parameters)}");
        await Clients.Group($"Client_{clientId}").ReceiveRemoteCommand(commandData);
    }

    public async Task LockClient(int clientId)
    {
        await Clients.Group($"Client_{clientId}").LockWorkstation();
        _logger.LogInformation($"Lock command sent to client {clientId}");
    }

    public async Task UnlockClient(int clientId)
    {
        await Clients.Group($"Client_{clientId}").UnlockWorkstation();
        _logger.LogInformation($"Unlock command sent to client {clientId}");
    }

    public async Task SendTextMessageToClient(int clientId, string message)
    {
        await Clients.Group($"Client_{clientId}").ReceiveTextMessage(message);
        _logger.LogInformation($"Text message sent to client {clientId}: {message}");
    }

    public async Task SetClientTimeWarning(int clientId, int minutes)
    {
        await Clients.Group($"Client_{clientId}").SetTimeWarning(minutes);
        _logger.LogInformation($"Time warning sent to client {clientId}: {minutes} minutes");
    }

    public async Task StartRemoteControlForClient(int clientId)
    {
        await Clients.Group($"Client_{clientId}").StartRemoteControl();
        _logger.LogInformation($"Remote control start command sent to client {clientId}");
    }

    public async Task StopRemoteControlForClient(int clientId)
    {
        await Clients.Group($"Client_{clientId}").StopRemoteControl();
        _logger.LogInformation($"Remote control stop command sent to client {clientId}");
    }
}

public class ClientStatusDto
{
    public int ClientId { get; set; }
    public bool IsConnected { get; set; }
    public int ConnectionCount { get; set; }
}