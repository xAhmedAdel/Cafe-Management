using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CafeManagement.Core.Interfaces;
using CafeManagement.Infrastructure.Data;
using CafeManagement.Infrastructure.Services;
using CafeManagement.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CafeManagementDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<CafeManagementDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IPasswordHasher<Core.Entities.User>, PasswordHasher<Core.Entities.User>>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IDeploymentService, DeploymentService>();

        // Ordering System Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderNotificationService, OrderNotificationService>();

        return services;
    }
}