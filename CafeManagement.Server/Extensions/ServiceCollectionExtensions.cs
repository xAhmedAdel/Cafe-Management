using CafeManagement.Core.Interfaces;
using CafeManagement.Server.Services;
using CafeManagement.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using CafeManagement.Server.Hubs;

namespace CafeManagement.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCafeManagementServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationService, NotificationService>();

        // No need to re-register ISessionService and IClientService
        // since they're already registered by AddInfrastructure()

        return services;
    }
}