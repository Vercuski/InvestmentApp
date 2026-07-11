using InvestmentApp.Application.Calculators;
using InvestmentApp.Application.Exceptions;
using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvestmentApp.Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationRegistration(this IHostApplicationBuilder builder)
    {
        builder.AddOptionsRegistration();
        builder.AddMediatorRegistration();
        builder.AddErrorHandling();
        builder.AddCalculations();
        return builder;
    }

    private static IHostApplicationBuilder AddCalculations(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<MacdCalculator>();
        builder.Services.AddScoped<RsiCalculator>();
        builder.Services.AddScoped<BollingerBandsCalculator>();
        builder.Services.AddScoped<ObvCalculator>();
        builder.Services.AddScoped<StochasticCalculator>();
        builder.Services.AddScoped<AdxCalculator>();
        builder.Services.AddScoped<AtrCalculator>();
        builder.Services.AddScoped<CciCalculator>();
        builder.Services.AddScoped<ChaikinMoneyFlowCalculator>();
        builder.Services.AddScoped<KeltnerChannelsCalculator>();
        builder.Services.AddScoped<MovingAverageCrossoverCalculator>();
        builder.Services.AddScoped<SignalAggregator>();
        return builder;
    }


    private static IHostApplicationBuilder AddErrorHandling(this IHostApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    private static IHostApplicationBuilder AddMediatorRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        });
        return builder;
    }

    private static IHostApplicationBuilder AddOptionsRegistration(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<SwaggerOptions>(GetSection<SwaggerOptions>(builder.Configuration));
        builder.Services.Configure<ConnectionStringOptions>(GetSection<ConnectionStringOptions>(builder.Configuration));
        builder.Services.Configure<LogOptions>(GetSection<LogOptions>(builder.Configuration));
        return builder;
    }

    private static IConfigurationSection GetSection<T>(IConfiguration configuration)
        where T : IBaseOptionsConfig
    {
        var config = Activator.CreateInstance<T>()!;
        var section = ((IBaseOptionsConfig)config).Section;
        return configuration.GetSection(section);
    }
}
