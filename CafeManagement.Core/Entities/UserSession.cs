using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Core.Entities;

public class UserSession
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string SessionToken { get; set; } = "";

    [Required]
    public string RefreshToken { get; set; } = "";

    public string? UserAgent { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }
}