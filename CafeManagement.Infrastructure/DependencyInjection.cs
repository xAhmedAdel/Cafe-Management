using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CafeManagement.Core.Interfaces;
using CafeManagement.Infrastructure.Data;
using CafeManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CafeManagementDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IPasswordHasher<Core.Entities.User>, PasswordHasher<Core.Entities.User>>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}