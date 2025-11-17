using CafeManagement.Core.Entities;

namespace CafeManagement.Core.Interfaces;

public interface IDeploymentService
{
    Task<IEnumerable<ClientDeployment>> GetAllDeploymentsAsync();
    Task<ClientDeployment?> GetDeploymentByIdAsync(int id);
    Task<ClientDeployment?> GetDeploymentByIpAsync(string ipAddress);
    Task<ClientDeployment> CreateDeploymentAsync(ClientDeployment deployment);
    Task<ClientDeployment> UpdateDeploymentAsync(ClientDeployment deployment);
    Task<bool> DeleteDeploymentAsync(int id);
    Task<bool> DeployToClientAsync(int deploymentId, string targetVersion, string performedBy);
    Task<bool> CheckClientStatusAsync(int deploymentId);
    Task<IEnumerable<DeploymentLog>> GetDeploymentLogsAsync(int deploymentId, int limit = 50);
    Task<ClientDeployment> RegisterClientAsync(string clientName, string ipAddress, string macAddress, string location);
    Task<IEnumerable<ClientDeployment>> GetOfflineDeploymentsAsync(TimeSpan offlineThreshold);
    Task<bool> UpdateClientVersionAsync(int deploymentId, string newVersion);
    Task<DeploymentStatistics> GetDeploymentStatisticsAsync();
}

public class DeploymentStatistics
{
    public int TotalDeployments { get; set; }
    public int OnlineDeployments { get; set; }
    public int OfflineDeployments { get; set; }
    public int PendingDeployments { get; set; }
    public int DeployingDeployments { get; set; }
    public int ErrorDeployments { get; set; }
    public decimal UptimePercentage { get; set; }
    public Dictionary<DeploymentStatus, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> VersionDistribution { get; set; } = new();
}