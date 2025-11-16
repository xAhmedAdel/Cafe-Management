using CafeManagement.Core.Interfaces;
using CafeManagement.Server.Services;
using CafeManagement.Server.Handlers;
using Microsoft.AspNetCore.SignalR;
using CafeManagement.Server.Hubs;

namespace CafeManagement.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCafeManagementServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationService, NotificationService>();

        // Replace core services with SignalR-enabled versions
        services.AddScoped<ISessionService>(provider =>
        {
            var innerService = provider.GetRequiredService<CafeManagement.Infrastructure.Services.SessionService>();
            var notificationService = provider.GetRequiredService<INotificationService>();
            var mapper = provider.GetRequiredService<AutoMapper.IMapper>();
            return new SignalREnabledSessionService(innerService, notificationService, mapper);
        });

        services.AddScoped<IClientService>(provider =>
        {
            var innerService = provider.GetRequiredService<CafeManagement.Infrastructure.Services.ClientService>();
            var notificationService = provider.GetRequiredService<INotificationService>();
            var mapper = provider.GetRequiredService<AutoMapper.IMapper>();
            return new SignalREnabledClientService(innerService, notificationService, mapper);
        });

        return services;
    }
}