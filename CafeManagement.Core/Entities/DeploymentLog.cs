using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Core.Entities;

public class DeploymentLog
{
    public int Id { get; set; }

    public int ClientDeploymentId { get; set; }
    public ClientDeployment ClientDeployment { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = "";

    [Required]
    public DeploymentLogLevel Level { get; set; } = DeploymentLogLevel.Info;

    [Required]
    public string Message { get; set; } = "";

    public string? Details { get; set; }

    public bool Success { get; set; } = true;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? PerformedBy { get; set; }
}

public enum DeploymentLogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}