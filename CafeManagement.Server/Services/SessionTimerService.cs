using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using CafeManagement.Server.Hubs;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.Enums;
using System.Collections.Concurrent;

namespace CafeManagement.Server.Services;

public class SessionTimerService : BackgroundService
{
    private readonly ILogger<SessionTimerService> _logger;
    private readonly IHubContext<CafeManagementHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly Timer _timer;

    public SessionTimerService(
        ILogger<SessionTimerService> logger,
        IHubContext<CafeManagementHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;

        // Check every 30 seconds
        _timer = new Timer(CheckExpiredSessions, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Timer Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("Session Timer Service stopped");
    }

    private async void CheckExpiredSessions(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

            var expiredSessions = await sessionService.GetExpiredSessionsAsync();

            foreach (var session in expiredSessions)
            {
                _logger.LogInformation($"Session {session.Id} for client {session.ClientId} has expired");

                // Send lock command to client
                await _hubContext.Clients.All.SendAsync("LockWorkstation");

                // Update session status to completed
                session.Status = SessionStatus.Completed;
                session.EndTime = DateTime.UtcNow;
                await sessionService.UpdateSessionAsync(session);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking expired sessions");
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}