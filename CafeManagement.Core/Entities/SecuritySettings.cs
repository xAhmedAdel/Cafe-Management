using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Core.Entities;

public class SecuritySettings
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SettingKey { get; set; } = "";

    [Required]
    public string SettingValue { get; set; } = "";

    [StringLength(200)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class SecuritySettingKeys
{
    public const string PasswordMinLength = "PasswordMinLength";
    public const string PasswordRequireUppercase = "PasswordRequireUppercase";
    public const string PasswordRequireLowercase = "PasswordRequireLowercase";
    public const string PasswordRequireNumbers = "PasswordRequireNumbers";
    public const string PasswordRequireSpecialChars = "PasswordRequireSpecialChars";
    public const string MaxFailedLoginAttempts = "MaxFailedLoginAttempts";
    public const string AccountLockoutDuration = "AccountLockoutDuration";
    public const string SessionTimeoutMinutes = "SessionTimeoutMinutes";
    public const string RequireTwoFactorAuth = "RequireTwoFactorAuth";
    public const string PasswordExpiryDays = "PasswordExpiryDays";
    public const string ForcePasswordChangeOnFirstLogin = "ForcePasswordChangeOnFirstLogin";
}