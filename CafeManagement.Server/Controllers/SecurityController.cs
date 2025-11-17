// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using CafeManagement.Application.Services;

// namespace CafeManagement.Server.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// [Authorize(Roles = "Admin")]
// public class SecurityController : ControllerBase
// {
//     private readonly ISecurityService _securityService;
//     private readonly ILogger<SecurityController> _logger;

//     public SecurityController(ISecurityService securityService, ILogger<SecurityController> logger)
//     {
//         _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }

    //     [HttpGet("settings")]
//     public async Task<ActionResult> GetSecuritySettings()
//     {
//         try
//         {
//             var settings = await _securityService.GetAllSecuritySettingsAsync();
//             return Ok(settings);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving security settings");
//             return StatusCode(500, "Internal server error while retrieving security settings");
//         }
//     }

//     [HttpPut("settings")]
//     public async Task<ActionResult> UpdateSecuritySettings([FromBody] Dictionary<string, string> settings)
//     {
//         try
//         {
//             foreach (var setting in settings)
//             {
//                 await _securityService.UpdateSecuritySettingAsync(setting.Key, setting.Value);
//             }

//             return Ok(new { message = "Security settings updated successfully" });
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating security settings");
//             return StatusCode(500, "Internal server error while updating security settings");
//         }
//     }

//     [HttpGet("audit-logs")]
//     public async Task<ActionResult> GetAuditLogs(
//         [FromQuery] DateTime? startDate = null,
//         [FromQuery] DateTime? endDate = null,
//         [FromQuery] int? userId = null,
//         [FromQuery] string? action = null,
//         [FromQuery] string? entityType = null,
//         [FromQuery] int limit = 100)
//     {
//         try
//         {
//             var logs = await _securityService.GetAuditLogsAsync(startDate, endDate, userId, action, entityType, limit);
//             return Ok(logs);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving audit logs");
//             return StatusCode(500, "Internal server error while retrieving audit logs");
//         }
//     }

//     [HttpGet("sessions/{userId}")]
//     public async Task<ActionResult> GetUserSessions(int userId)
//     {
//         try
//         {
//             var sessions = await _securityService.GetUserSessionsAsync(userId);
//             return Ok(sessions);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving user sessions for user {UserId}", userId);
//             return StatusCode(500, $"Internal server error while retrieving user sessions for user {userId}");
//         }
//     }

//     [HttpPost("sessions/{userId}/revoke")]
//     public async Task<ActionResult> RevokeAllUserSessions(int userId, [FromBody] RevokeSessionsRequest request)
//     {
//         try
//         {
//             await _securityService.RevokeAllUserSessionsAsync(userId, request?.Reason);
//             return Ok(new { message = "All user sessions revoked successfully" });
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error revoking sessions for user {UserId}", userId);
//             return StatusCode(500, $"Internal server error while revoking sessions for user {userId}");
//         }
//     }

//     [HttpPost("cleanup-sessions")]
//     public async Task<ActionResult> CleanupExpiredSessions()
//     {
//         try
//         {
//             await _securityService.CleanupExpiredSessionsAsync();
//             return Ok(new { message = "Expired sessions cleanup completed" });
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error during expired sessions cleanup");
//             return StatusCode(500, "Internal server error during expired sessions cleanup");
//         }
//     }
// }

// public class RevokeSessionsRequest
// {
//     public string? Reason { get; set; }
// }