using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CafeManagement.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CafeManagementDbContext>
{
    public CafeManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CafeManagementDbContext>();
        optionsBuilder.UseSqlite("Data Source=cafemanagement.db");

        return new CafeManagementDbContext(optionsBuilder.Options);
    }
}