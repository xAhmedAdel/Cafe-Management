using CafeManagement.Core.Interfaces;
using CafeManagement.Server.Services;
using CafeManagement.Infrastructure.Services;
using CafeManagement.Application.Services;
using Microsoft.AspNetCore.SignalR;
using CafeManagement.Server.Hubs;

namespace CafeManagement.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCafeManagementServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register authentication service
        services.AddScoped<IAuthService, AuthService>();

        // Register billing service
        services.AddScoped<IBillingService, BillingService>();

        // Register reporting service
        services.AddScoped<IReportService, ReportService>();

        // No need to re-register ISessionService and IClientService
        // since they're already registered by AddInfrastructure()

        return services;
    }
}