using Hangfire;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Services;
using InvestmentApp.Infrastructure.HealthChecks;
using InvestmentApp.Infrastructure.Repositories;
using InvestmentApp.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
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
        builder.AddServicesRegistration();
        builder.AddVPNRegistration();
        builder.AddHealthChecksRegistration();
        builder.AddLoggingRegistration();
        builder.AddRepositoriesRegistration();
        builder.AddHangfire();
        return builder;
    }
 
    private static IHostApplicationBuilder AddHangfire(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage("Server=localhost;Database=stocks;User Id=sa;Password=Password123;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True"));
        builder.Services.AddHangfireServer();
        return builder;
    }

    private static IHostApplicationBuilder AddRepositoriesRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStockDataRepository, StockDataRepository>();
        builder.Services.AddScoped<ITradeSignalPointRepository, TradeSignalPointRepository>();
        builder.Services.AddScoped<ITickerRepository, TickerRepository>();
        builder.Services.AddScoped<IExchangeRepository, ExchangeRepository>();
        builder.Services.AddScoped<IPositionRepository, PositionRepository>();
        return builder;
    }

    private static IHostApplicationBuilder AddServicesRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IDataDownloadService, DataDownloadService>(client =>
            client.BaseAddress = new Uri("https://query2.finance.yahoo.com/")
        );
        builder.Services.AddHttpClient("localhost", client =>
        {
            client.BaseAddress = new Uri("https://localhost:8080/");
        });
        builder.Services.AddScoped<IEodDataScraperService, EodDataScraperService>();
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

    private static IHostApplicationBuilder AddVPNRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IVPNService, VPNService>();
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
