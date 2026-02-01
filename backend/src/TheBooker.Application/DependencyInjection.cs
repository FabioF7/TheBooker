using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TheBooker.Application.Common.Behaviors;
using TheBooker.Application.Services.Availability;

namespace TheBooker.Application;

/// <summary>
/// Dependency injection configuration for Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Services
        services.AddScoped<IAvailabilityService, AvailabilityService>();

        return services;
    }
}
