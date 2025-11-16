using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;
using CafeManagement.Infrastructure.Data;
using CafeManagement.Infrastructure.Repositories;

namespace CafeManagement.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CafeManagementDbContext _context;
    private IRepository<Client>? _clients;
    private IRepository<User>? _users;
    private IRepository<Session>? _sessions;
    private IRepository<LockScreenConfig>? _lockScreenConfigs;
    private IRepository<UsageLog>? _usageLogs;

    public UnitOfWork(CafeManagementDbContext context)
    {
        _context = context;
    }

    public IRepository<Client> Clients => _clients ??= new Repository<Client>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Session> Sessions => _sessions ??= new Repository<Session>(_context);
    public IRepository<LockScreenConfig> LockScreenConfigs => _lockScreenConfigs ??= new Repository<LockScreenConfig>(_context);
    public IRepository<UsageLog> UsageLogs => _usageLogs ??= new Repository<UsageLog>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}