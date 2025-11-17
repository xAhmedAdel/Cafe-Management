using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Core.Entities;

public class ClientDeployment
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = "";

    [Required]
    [StringLength(15)]
    public string IpAddress { get; set; } = "";

    [StringLength(20)]
    public string MacAddress { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string Location { get; set; } = "";

    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public DateTime? LastDeployment { get; set; }

    public string Version { get; set; } = "1.0.0";

    public string TargetVersion { get; set; } = "1.0.0";

    public bool AutoUpdateEnabled { get; set; } = true;

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<DeploymentLog> DeploymentLogs { get; set; } = new();
}

public enum DeploymentStatus
{
    Pending,
    Deploying,
    Online,
    Offline,
    Error,
    Updating,
    Maintenance
}