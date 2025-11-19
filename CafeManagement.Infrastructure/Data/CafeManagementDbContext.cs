using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Infrastructure.Data;

public class CafeManagementDbContext : DbContext, IApplicationDbContext
{
    public CafeManagementDbContext(DbContextOptions<CafeManagementDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<LockScreenConfig> LockScreenConfigs { get; set; }
    public DbSet<UsageLog> UsageLogs { get; set; }
    public DbSet<BillingSettings> BillingSettings { get; set; }
    public DbSet<ClientDeployment> ClientDeployments { get; set; }
    public DbSet<DeploymentLog> DeploymentLogs { get; set; }

    // Ordering System DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderNotification> OrderNotifications { get; set; }

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

        modelBuilder.Entity<BillingSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HourlyRate).HasDefaultValue(20.00m);
            entity.Property(e => e.Currency).HasDefaultValue("L.E").HasMaxLength(10);
            entity.Property(e => e.MinimumSessionDuration).HasDefaultValue("1 hour").HasMaxLength(50);
            entity.Property(e => e.RoundUpToNearestHour).HasDefaultValue(true);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ClientDeployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(15);
            entity.Property(e => e.MacAddress).HasMaxLength(20);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Version).HasDefaultValue("1.0.0").HasMaxLength(20);
            entity.Property(e => e.TargetVersion).HasDefaultValue("1.0.0").HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(DeploymentStatus.Pending);
            entity.Property(e => e.AutoUpdateEnabled).HasDefaultValue(true);
            entity.HasOne(e => e.Client)
                  .WithOne()
                  .HasForeignKey<ClientDeployment>(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DeploymentLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Level).HasDefaultValue(DeploymentLogLevel.Info);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Success).HasDefaultValue(true);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.ClientDeployment)
                  .WithMany(d => d.DeploymentLogs)
                  .HasForeignKey(e => e.ClientDeploymentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Ordering System Configurations
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasDefaultValue(0.00m);
            entity.Property(e => e.Category).HasDefaultValue(ProductCategory.Drinks);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.PreparationTimeMinutes).HasDefaultValue(5);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasIndex(e => e.DisplayOrder);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasDefaultValue(0.00m);
            entity.Property(e => e.Status).HasDefaultValue(OrderStatus.Pending);
            entity.Property(e => e.CustomerNotes).HasMaxLength(500);
            entity.Property(e => e.CompletedAt).IsRequired(false);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Client)
                  .WithMany()
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasDefaultValue(0.00m);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
        });

        modelBuilder.Entity<OrderNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasDefaultValue(NotificationType.NewOrder);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.ReadAt).IsRequired(false);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Notifications)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Admin)
                  .WithMany()
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.AdminId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.Type);
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

        var defaultBillingSettings = new BillingSettings
        {
            Id = 1,
            HourlyRate = 20.00m,
            Currency = "L.E",
            MinimumSessionDuration = "1 hour",
            RoundUpToNearestHour = true,
            Description = "Default billing configuration",
            IsActive = true,
            CreatedAt = fixedDateTime,
            UpdatedAt = fixedDateTime
        };

        modelBuilder.Entity<BillingSettings>().HasData(defaultBillingSettings);

        // Seed sample products for the ordering system
        var products = new[]
        {
            // Drinks
            new Product { Id = 1, Name = "Espresso", Description = "Strong black coffee", Price = 2.50m, Category = ProductCategory.Drinks, DisplayOrder = 1, PreparationTimeMinutes = 3, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 2, Name = "Cappuccino", Description = "Espresso with steamed milk foam", Price = 3.50m, Category = ProductCategory.Drinks, DisplayOrder = 2, PreparationTimeMinutes = 4, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 3, Name = "Latte", Description = "Espresso with steamed milk", Price = 4.00m, Category = ProductCategory.Drinks, DisplayOrder = 3, PreparationTimeMinutes = 4, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 4, Name = "Tea", Description = "Hot tea selection", Price = 2.00m, Category = ProductCategory.Drinks, DisplayOrder = 4, PreparationTimeMinutes = 3, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 5, Name = "Fresh Juice", Description = "Orange or apple juice", Price = 3.00m, Category = ProductCategory.Drinks, DisplayOrder = 5, PreparationTimeMinutes = 2, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },

            // Food
            new Product { Id = 6, Name = "Sandwich", Description = "Fresh deli sandwich", Price = 6.00m, Category = ProductCategory.Food, DisplayOrder = 1, PreparationTimeMinutes = 8, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 7, Name = "Pizza Slice", Description = "Fresh pizza slice", Price = 4.50m, Category = ProductCategory.Food, DisplayOrder = 2, PreparationTimeMinutes = 10, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 8, Name = "Burger", Description = "Classic beef burger", Price = 8.00m, Category = ProductCategory.Food, DisplayOrder = 3, PreparationTimeMinutes = 12, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 9, Name = "Salad", Description = "Fresh garden salad", Price = 5.50m, Category = ProductCategory.Food, DisplayOrder = 4, PreparationTimeMinutes = 5, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },

            // Snacks
            new Product { Id = 10, Name = "Chips", Description = "Potato chips", Price = 1.50m, Category = ProductCategory.Snacks, DisplayOrder = 1, PreparationTimeMinutes = 1, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 11, Name = "Chocolate Bar", Description = "Various chocolate options", Price = 2.00m, Category = ProductCategory.Snacks, DisplayOrder = 2, PreparationTimeMinutes = 1, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 12, Name = "Cookies", Description = "Fresh baked cookies", Price = 2.50m, Category = ProductCategory.Snacks, DisplayOrder = 3, PreparationTimeMinutes = 2, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime },
            new Product { Id = 13, Name = "Cake Slice", Description = "Daily cake selection", Price = 3.50m, Category = ProductCategory.Snacks, DisplayOrder = 4, PreparationTimeMinutes = 2, CreatedAt = fixedDateTime, UpdatedAt = fixedDateTime }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}