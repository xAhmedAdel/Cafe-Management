using CafeManagement.Core.Entities;

namespace CafeManagement.Core.Interfaces;

public interface ISessionService
{
    Task<Session> StartSessionAsync(int clientId, int? userId, int durationMinutes);
    Task<Session> EndSessionAsync(int sessionId);
    Task<Session> ExtendSessionAsync(int sessionId, int additionalMinutes);
    Task<Session?> GetActiveSessionAsync(int clientId);
    Task<decimal> CalculateSessionCostAsync(int sessionId);
    Task<List<Session>> GetExpiredSessionsAsync();
    Task<Session> UpdateSessionAsync(Session session);
    Task<List<Session>> GetAllSessionsAsync();
    Task<List<Session>> GetActiveSessionsAsync();
}