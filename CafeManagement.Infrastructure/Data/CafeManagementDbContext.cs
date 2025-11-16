using CafeManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Infrastructure.Data;

public class CafeManagementDbContext : DbContext
{
    public CafeManagementDbContext(DbContextOptions<CafeManagementDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<LockScreenConfig> LockScreenConfigs { get; set; }
    public DbSet<UsageLog> UsageLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Balance).HasDefaultValue(0.00m);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.MACAddress).IsRequired().HasMaxLength(17);
            entity.Property(e => e.Status).HasDefaultValue(Core.Enums.ClientStatus.Offline);
            entity.HasIndex(e => e.MACAddress).IsUnique();
            entity.HasOne(e => e.CurrentSession)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentSessionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HourlyRate).HasDefaultValue(2.00m);
            entity.Property(e => e.TotalAmount).HasDefaultValue(0.00m);
            entity.Property(e => e.Status).HasDefaultValue(Core.Enums.SessionStatus.Active);
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Sessions)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Sessions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LockScreenConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BackgroundColor).HasDefaultValue("#000000");
            entity.Property(e => e.TextColor).HasDefaultValue("#FFFFFF");
            entity.Property(e => e.ShowTimeRemaining).HasDefaultValue(true);
            entity.HasOne(e => e.Client)
                  .WithOne(c => c.LockScreenConfig)
                  .HasForeignKey<LockScreenConfig>(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsageLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.UsageLogs)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UsageLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var fixedDateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q=", // admin123
            Email = "admin@cafemanagement.com",
            Role = Core.Enums.UserRole.Admin,
            Balance = 1000.00m,
            CreatedAt = fixedDateTime,
            UpdatedAt = fixedDateTime
        };

        modelBuilder.Entity<User>().HasData(adminUser);

        var operatorUser = new User
        {
            Id = 2,
            Username = "operator",
            PasswordHash = "AQAAAAEAACcQAAAAEKqgkTvtFvKFGMGj3QF4YZL3pqYOjOEgkKhfYxYU+0Q=", // operator123
            Email = "operator@cafemanagement.com",
            Role = Core.Enums.UserRole.Operator,
            Balance = 0.00m,
            CreatedAt = fixedDateTime,
            UpdatedAt = fixedDateTime
        };

        modelBuilder.Entity<User>().HasData(operatorUser);
    }
}