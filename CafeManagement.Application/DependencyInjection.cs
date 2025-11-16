using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using CafeManagement.Core.Interfaces;
using FluentValidation;
using MediatR;

namespace CafeManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

        return services;
    }
}