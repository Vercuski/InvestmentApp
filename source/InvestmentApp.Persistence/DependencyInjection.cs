using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Persistence.ConnectionFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvestmentApp.Persistence;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPersistenceRegistrations(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDbConnectionFactory, SqlDbConnectionFactory>();

        return builder;
    }
}
