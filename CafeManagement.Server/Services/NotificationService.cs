using CafeManagement.Application.DTOs;
using CafeManagement.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagement.Server.Services;

public interface INotificationService
{
    Task NotifyClientStatusUpdate(ClientDto client);
    Task NotifySessionStarted(SessionDto session);
    Task NotifySessionEnded(SessionDto session);
    Task NotifySessionExtended(SessionDto session);
    Task NotifyUserBalanceUpdated(UserDto user);
    Task BroadcastSystemMessage(string message, string targetGroup = "All");
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<CafeManagementHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<CafeManagementHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyClientStatusUpdate(ClientDto client)
    {
        try
        {
            await _hubContext.Clients.Group("Operators").SendAsync("ClientStatusUpdated", client);
            await _hubContext.Clients.Group($"Client_{client.Id}").SendAsync("ClientStatusUpdated", client);
            _logger.LogInformation($"Notified clients about status update for client {client.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying clients about client {client.Id} status update");
        }
    }

    public async Task NotifySessionStarted(SessionDto session)
    {
        try
        {
            await _hubContext.Clients.Group("Operators").SendAsync("SessionStarted", session);
            await _hubContext.Clients.Group($"Client_{session.ClientId}").SendAsync("SessionStarted", session);
            _logger.LogInformation($"Notified clients about session start for session {session.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying clients about session {session.Id} start");
        }
    }

    public async Task NotifySessionEnded(SessionDto session)
    {
        try
        {
            await _hubContext.Clients.Group("Operators").SendAsync("SessionEnded", session);
            await _hubContext.Clients.Group($"Client_{session.ClientId}").SendAsync("SessionEnded", session);
            _logger.LogInformation($"Notified clients about session end for session {session.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying clients about session {session.Id} end");
        }
    }

    public async Task NotifySessionExtended(SessionDto session)
    {
        try
        {
            await _hubContext.Clients.Group("Operators").SendAsync("SessionExtended", session);
            await _hubContext.Clients.Group($"Client_{session.ClientId}").SendAsync("SessionExtended", session);
            _logger.LogInformation($"Notified clients about session extension for session {session.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying clients about session {session.Id} extension");
        }
    }

    public async Task NotifyUserBalanceUpdated(UserDto user)
    {
        try
        {
            await _hubContext.Clients.Group("Operators").SendAsync("UserBalanceUpdated", user);
            await _hubContext.Clients.Group($"Client_{user.Id}").SendAsync("UserBalanceUpdated", user);
            _logger.LogInformation($"Notified clients about balance update for user {user.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying clients about user {user.Id} balance update");
        }
    }

    public async Task BroadcastSystemMessage(string message, string targetGroup = "All")
    {
        try
        {
            if (targetGroup == "All")
            {
                await _hubContext.Clients.All.SendAsync("SystemMessage", message);
            }
            else if (targetGroup == "Operators")
            {
                await _hubContext.Clients.Group("Operators").SendAsync("SystemMessage", message);
            }
            else if (targetGroup == "Administrators")
            {
                await _hubContext.Clients.Group("Administrators").SendAsync("SystemMessage", message);
            }

            _logger.LogInformation($"Broadcasted system message to {targetGroup}: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting system message to {targetGroup}");
        }
    }
}