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
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
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
        app.AddRecurringJobs();
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
        builder.Services.AddScoped<IEodDataScraperService, EodDataScraperService>();
        builder.Services.AddScoped<IScheduledJobsService, ScheduledJobsService>();
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
        builder.Services.AddScoped<IVpnService, VPNService>();
        return builder;
    }

    private static IHostApplicationBuilder AddLoggingRegistration(this IHostApplicationBuilder builder)
    {
        var commandDbConnectionString = builder.Configuration.GetConnectionString("CommandDbConnection")!;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.MSSqlServer(
                connectionString: commandDbConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                });

        if (!builder.Environment.IsProduction())
        {
            loggerConfiguration.WriteTo.Console();
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Services.AddSerilog(dispose: true);
        return builder;
    }

    private static void AddRecurringJobs(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

        // Job 1 + 2: download all stock data, then run calculations once that finishes.
        // Weekdays at 6:00 PM Eastern (handles EST/EDT automatically via the IANA time zone).
        recurringJobManager.AddOrUpdate<IScheduledJobsService>(
            "daily-download-and-calculate",
            job => job.RunDailyDownloadAndCalculationAsync(),
            "0 18 * * 1-5",
            new RecurringJobOptions { TimeZone = easternTimeZone });

        // Job 3: download open-position stock data every 30 minutes, 9:30 AM - 4:00 PM Eastern, weekdays.
        // Split into three cron registrations so the half-hour grid lands exactly on 9:30 and 4:00
        // instead of also firing at 9:00 or 4:30.
        recurringJobManager.AddOrUpdate<IScheduledJobsService>(
            "open-position-download-market-open",
            job => job.DownloadOpenPositionStockDataAsync(),
            "30 9 * * 1-5",
            new RecurringJobOptions { TimeZone = easternTimeZone });

        recurringJobManager.AddOrUpdate<IScheduledJobsService>(
            "open-position-download-intraday",
            job => job.DownloadOpenPositionStockDataAsync(),
            "0,30 10-15 * * 1-5",
            new RecurringJobOptions { TimeZone = easternTimeZone });

        recurringJobManager.AddOrUpdate<IScheduledJobsService>(
            "open-position-download-market-close",
            job => job.DownloadOpenPositionStockDataAsync(),
            "0 16 * * 1-5",
            new RecurringJobOptions { TimeZone = easternTimeZone });
    }
}
