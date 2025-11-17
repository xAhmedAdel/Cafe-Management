using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = "";

    [Required]
    public string EntityType { get; set; } = "";

    public int? EntityId { get; set; }

    public string? EntityIdentifier { get; set; }

    [StringLength(1000)]
    public string? OldValues { get; set; }

    [StringLength(1000)]
    public string? NewValues { get; set; }

    [StringLength(100)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool Success { get; set; } = true;

    [StringLength(500)]
    public string? ErrorMessage { get; set; }
}

public static class AuditActions
{
    public const string CREATE = "Create";
    public const string UPDATE = "Update";
    public const string DELETE = "Delete";
    public const string LOGIN = "Login";
    public const string LOGOUT = "Logout";
    public const string PASSWORD_CHANGE = "PasswordChange";
    public const string ROLE_CHANGE = "RoleChange";
    public const string SESSION_START = "SessionStart";
    public const string SESSION_END = "SessionEnd";
    public const string DEPLOY = "Deploy";
    public const string EXPORT = "Export";
    public const string ACCESS_DENIED = "AccessDenied";
}