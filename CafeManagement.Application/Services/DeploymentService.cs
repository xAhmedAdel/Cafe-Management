using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;
using System.Net.NetworkInformation;

namespace CafeManagement.Application.Services;

public class DeploymentService : IDeploymentService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeploymentService> _logger;
    private readonly TimeSpan OFFLINE_THRESHOLD = TimeSpan.FromMinutes(5);

    public DeploymentService(IApplicationDbContext context, ILogger<DeploymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDeployment>> GetAllDeploymentsAsync()
    {
        return await _context.ClientDeployments
            .Include(d => d.Client)
            .OrderBy(d => d.ClientName)
            .ToListAsync();
    }

    public async Task<ClientDeployment?> GetDeploymentByIdAsync(int id)
    {
        return await _context.ClientDeployments
            .Include(d => d.Client)
            .Include(d => d.DeploymentLogs.OrderByDescending(l => l.Timestamp))
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<ClientDeployment?> GetDeploymentByIpAsync(string ipAddress)
    {
        return await _context.ClientDeployments
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);
    }

    public async Task<ClientDeployment> CreateDeploymentAsync(ClientDeployment deployment)
    {
        _context.ClientDeployments.Add(deployment);
        await _context.SaveChangesAsync();

        await LogDeploymentAction(deployment.Id, "Created", DeploymentLogLevel.Info,
            $"New deployment created for {deployment.ClientName}");

        _logger.LogInformation($"Created deployment for client {deployment.ClientName} ({deployment.IpAddress})");
        return deployment;
    }

    public async Task<ClientDeployment> UpdateDeploymentAsync(ClientDeployment deployment)
    {
        deployment.UpdatedAt = DateTime.UtcNow;
        _context.ClientDeployments.Update(deployment);
        await _context.SaveChangesAsync();

        await LogDeploymentAction(deployment.Id, "Updated", DeploymentLogLevel.Info,
            $"Deployment information updated");

        _logger.LogInformation($"Updated deployment for client {deployment.ClientName}");
        return deployment;
    }

    public async Task<bool> DeleteDeploymentAsync(int id)
    {
        var deployment = await _context.ClientDeployments.FindAsync(id);
        if (deployment == null) return false;

        _context.ClientDeployments.Remove(deployment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted deployment for client {deployment.ClientName}");
        return true;
    }

    public async Task<bool> DeployToClientAsync(int deploymentId, string targetVersion, string performedBy)
    {
        var deployment = await _context.ClientDeployments.FindAsync(deploymentId);
        if (deployment == null) return false;

        try
        {
            deployment.TargetVersion = targetVersion;
            deployment.Status = DeploymentStatus.Deploying;
            deployment.LastDeployment = DateTime.UtcNow;
            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogDeploymentAction(deploymentId, "Deploy Started", DeploymentLogLevel.Info,
                $"Deployment to version {targetVersion} started", performedBy);

            var success = await PerformDeployment(deployment);

            if (success)
            {
                deployment.Status = DeploymentStatus.Online;
                deployment.Version = targetVersion;
                deployment.LastSeen = DateTime.UtcNow;
                await LogDeploymentAction(deploymentId, "Deploy Completed", DeploymentLogLevel.Info,
                    $"Successfully deployed to version {targetVersion}", performedBy);
            }
            else
            {
                deployment.Status = DeploymentStatus.Error;
                await LogDeploymentAction(deploymentId, "Deploy Failed", DeploymentLogLevel.Error,
                    $"Failed to deploy to version {targetVersion}", performedBy);
            }

            await _context.SaveChangesAsync();
            return success;
        }
        catch (Exception ex)
        {
            deployment.Status = DeploymentStatus.Error;
            await _context.SaveChangesAsync();

            await LogDeploymentAction(deploymentId, "Deploy Error", DeploymentLogLevel.Critical,
                $"Deployment error: {ex.Message}", performedBy);

            _logger.LogError(ex, $"Deployment failed for client {deployment.ClientName}");
            return false;
        }
    }

    public async Task<bool> CheckClientStatusAsync(int deploymentId)
    {
        var deployment = await _context.ClientDeployments.FindAsync(deploymentId);
        if (deployment == null) return false;

        try
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(deployment.IpAddress, 1000);

            if (reply.Status == IPStatus.Success)
            {
                if (deployment.Status == DeploymentStatus.Offline)
                {
                    await LogDeploymentAction(deploymentId, "Client Online", DeploymentLogLevel.Info,
                        "Client is back online");
                }

                deployment.Status = DeploymentStatus.Online;
                deployment.LastSeen = DateTime.UtcNow;
            }
            else
            {
                if (deployment.Status == DeploymentStatus.Online)
                {
                    await LogDeploymentAction(deploymentId, "Client Offline", DeploymentLogLevel.Warning,
                        "Client is not responding");
                }

                deployment.Status = DeploymentStatus.Offline;
            }

            deployment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Status check failed for client {deployment.ClientName}");
            return false;
        }
    }

    public async Task<IEnumerable<DeploymentLog>> GetDeploymentLogsAsync(int deploymentId, int limit = 50)
    {
        return await _context.DeploymentLogs
            .Where(l => l.ClientDeploymentId == deploymentId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<ClientDeployment> RegisterClientAsync(string clientName, string ipAddress, string macAddress, string location)
    {
        var existingDeployment = await GetDeploymentByIpAsync(ipAddress);

        if (existingDeployment != null)
        {
            existingDeployment.ClientName = clientName;
            existingDeployment.MacAddress = macAddress;
            existingDeployment.Location = location;
            existingDeployment.LastSeen = DateTime.UtcNow;
            existingDeployment.Status = DeploymentStatus.Online;
            existingDeployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LogDeploymentAction(existingDeployment.Id, "Client Re-registered", DeploymentLogLevel.Info,
                $"Client re-registered with updated information");

            return existingDeployment;
        }

        var deployment = new ClientDeployment
        {
            ClientName = clientName,
            IpAddress = ipAddress,
            MacAddress = macAddress,
            Location = location,
            Status = DeploymentStatus.Online,
            LastSeen = DateTime.UtcNow
        };

        await CreateDeploymentAsync(deployment);
        return deployment;
    }

    public async Task<IEnumerable<ClientDeployment>> GetOfflineDeploymentsAsync(TimeSpan offlineThreshold)
    {
        var threshold = DateTime.UtcNow - offlineThreshold;
        return await _context.ClientDeployments
            .Include(d => d.Client)
            .Where(d => d.LastSeen < threshold && d.Status != DeploymentStatus.Offline)
            .ToListAsync();
    }

    public async Task<bool> UpdateClientVersionAsync(int deploymentId, string newVersion)
    {
        var deployment = await _context.ClientDeployments.FindAsync(deploymentId);
        if (deployment == null) return false;

        var oldVersion = deployment.Version;
        deployment.Version = newVersion;
        deployment.LastSeen = DateTime.UtcNow;
        deployment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await LogDeploymentAction(deploymentId, "Version Updated", DeploymentLogLevel.Info,
            $"Client version updated from {oldVersion} to {newVersion}");

        return true;
    }

    public async Task<DeploymentStatistics> GetDeploymentStatisticsAsync()
    {
        var deployments = await _context.ClientDeployments.ToListAsync();

        var stats = new DeploymentStatistics
        {
            TotalDeployments = deployments.Count,
            OnlineDeployments = deployments.Count(d => d.Status == DeploymentStatus.Online),
            OfflineDeployments = deployments.Count(d => d.Status == DeploymentStatus.Offline),
            PendingDeployments = deployments.Count(d => d.Status == DeploymentStatus.Pending),
            DeployingDeployments = deployments.Count(d => d.Status == DeploymentStatus.Deploying),
            ErrorDeployments = deployments.Count(d => d.Status == DeploymentStatus.Error)
        };

        stats.UptimePercentage = deployments.Count > 0
            ? (decimal)stats.OnlineDeployments / deployments.Count * 100
            : 0;

        stats.StatusDistribution = deployments
            .GroupBy(d => d.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.VersionDistribution = deployments
            .GroupBy(d => d.Version)
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    private async Task<bool> PerformDeployment(ClientDeployment deployment)
    {
        await Task.Delay(2000);

        var ping = new Ping();
        var reply = await ping.SendPingAsync(deployment.IpAddress, 2000);

        return reply.Status == IPStatus.Success;
    }

    private async Task LogDeploymentAction(int deploymentId, string action, DeploymentLogLevel level,
        string message, string? performedBy = null)
    {
        var log = new DeploymentLog
        {
            ClientDeploymentId = deploymentId,
            Action = action,
            Level = level,
            Message = message,
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        };

        _context.DeploymentLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}