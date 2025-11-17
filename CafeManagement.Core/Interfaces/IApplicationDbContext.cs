using Microsoft.EntityFrameworkCore;
using CafeManagement.Core.Entities;

namespace CafeManagement.Core.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Client> Clients { get; }
    DbSet<Session> Sessions { get; }
    DbSet<UsageLog> UsageLogs { get; }
    DbSet<BillingSettings> BillingSettings { get; }
    DbSet<ClientDeployment> ClientDeployments { get; }
    DbSet<DeploymentLog> DeploymentLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}