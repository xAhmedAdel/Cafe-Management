using CafeManagement.Core.Entities;

namespace CafeManagement.Core.Interfaces;

public interface ISecurityService
{
    Task<UserSession> CreateUserSessionAsync(int userId, string sessionToken, string refreshToken, string? userAgent = null, string? ipAddress = null);
    Task<UserSession?> GetUserSessionAsync(string sessionToken);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task RevokeSessionAsync(string sessionToken, string? reason = null);
    Task RevokeAllUserSessionsAsync(int userId, string? reason = null);
    Task CleanupExpiredSessionsAsync();
    Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
    Task LogAuditEventAsync(int? userId, string action, string entityType, int? entityId = null, string? entityIdentifier = null, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, bool success = true, string? errorMessage = null);
    Task<SecuritySettings?> GetSecuritySettingAsync(string key);
    Task<bool> ValidatePasswordPolicyAsync(string password);
    Task<bool> IsAccountLockedAsync(int userId);
    Task LockAccountAsync(int userId, string reason, TimeSpan duration);
    Task UnlockAccountAsync(int userId);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int? userId = null, string? action = null, string? entityType = null, int limit = 100);
    Task UpdateSecuritySettingAsync(string key, string value, string? description = null);
    Task<Dictionary<string, string>> GetAllSecuritySettingsAsync();
}