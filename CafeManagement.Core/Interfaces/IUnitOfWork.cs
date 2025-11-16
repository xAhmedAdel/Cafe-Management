namespace CafeManagement.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Entities.Client> Clients { get; }
    IRepository<Entities.User> Users { get; }
    IRepository<Entities.Session> Sessions { get; }
    IRepository<Entities.LockScreenConfig> LockScreenConfigs { get; }
    IRepository<Entities.UsageLog> UsageLogs { get; }

    Task<int> SaveChangesAsync();
}