using InvestmentApp.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace InvestmentApp.Infrastructure;

public static class DependencyInjection
{
    public static WebApplication? AddInfrastructureApplicationRegistration(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckConfiguration.WriteResponse
        });
        return app;
    }

    public static IHostApplicationBuilder AddInfrastructureRegistration(this IHostApplicationBuilder builder)
    {
        builder.AddHealthChecksRegistration();
        builder.AddLoggingRegistration();
        return builder;
    }

    private static IHostApplicationBuilder AddHealthChecksRegistration(this IHostApplicationBuilder builder)
    {
        var healthCheckBuilder = builder.Services.AddHealthChecks();
        foreach (var healthCheckType in Assembly.GetExecutingAssembly()
            .GetTypes().Where(type => !type.IsAbstract &&
            type.GetInterfaces().Contains(typeof(IHealthCheck))))
        {
            healthCheckBuilder.AddCheck(healthCheckType.Name,
                (IHealthCheck)Activator.CreateInstance(healthCheckType)!);
        }
        return builder;
    }

    private static IHostApplicationBuilder AddLoggingRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddLogging(config =>
        {
            config.ClearProviders();
            if (!builder.Environment.IsProduction())
            {
                config.AddConsole();
            }
        });
        return builder;
    }
}
