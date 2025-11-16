using CafeManagement.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagement.Server.Hubs;

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
}

public class CafeManagementHub : Hub<ICafeManagementHubClient>
{
    private readonly ILogger<CafeManagementHub> _logger;
    private static readonly Dictionary<string, int> _connectedClients = new();
    private static readonly Dictionary<int, HashSet<string>> _clientConnections = new();

    public CafeManagementHub(ILogger<CafeManagementHub> logger)
    {
        _logger = logger;
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

        // Remove from tracking
        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
            _connectedClients.Remove(connectionId);

            if (_clientConnections.TryGetValue(clientId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _clientConnections.Remove(clientId);
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

        _connectedClients[connectionId] = clientId;

        if (!_clientConnections.ContainsKey(clientId))
        {
            _clientConnections[clientId] = new HashSet<string>();
        }
        _clientConnections[clientId].Add(connectionId);

        // Add this connection to a group for the client
        await Groups.AddToGroupAsync(connectionId, $"Client_{clientId}");

        _logger.LogInformation($"Client {clientId} registered with connection {connectionId}");
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
        // Return list of connected clients with their status
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

    // Screen sharing methods
    public async Task ReceiveScreenshot(byte[] imageData)
    {
        var connectionId = Context.ConnectionId;

        // Find which client sent this screenshot
        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
            // Broadcast screenshot to all connected operators
            await Clients.Group("Operators").ReceiveScreenshot(imageData);
            await Clients.Group("Administrators").ReceiveScreenshot(imageData);

            _logger.LogDebug($"Screenshot received from client {clientId}: {imageData.Length} bytes");
        }
    }

    public async Task RemoteControlStarted()
    {
        var connectionId = Context.ConnectionId;

        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
            // Notify all operators that remote control has started
            await Clients.Group("Operators").SystemMessage($"Remote control started for client {clientId}");
            await Clients.Group("Administrators").SystemMessage($"Remote control started for client {clientId}");

            _logger.LogInformation($"Remote control started for client {clientId}");
        }
    }

    public async Task RemoteControlStopped()
    {
        var connectionId = Context.ConnectionId;

        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
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
        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
            // Send command to specific client
            await Clients.Group($"Client_{clientId}").ReceiveRemoteCommand(System.Text.Encoding.UTF8.GetBytes($"{command}:{string.Join(",", parameters)}"));

            _logger.LogDebug($"Remote command forwarded to client {clientId}: {command}");
        }
    }

    // Client control methods
    public async Task RequestUnlock()
    {
        var connectionId = Context.ConnectionId;

        if (_connectedClients.TryGetValue(connectionId, out var clientId))
        {
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