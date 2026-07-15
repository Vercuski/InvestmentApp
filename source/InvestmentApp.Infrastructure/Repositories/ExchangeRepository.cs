using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Infrastructure.Repositories;

public class ExchangeRepository(IDbConnectionFactory connectionFactory) : IExchangeRepository
{
    private const string GetExchangesSql =
        "SELECT ExchangeSymbol, ExchangeDescription, Active FROM Exchanges ORDER BY ExchangeSymbol";

    private const string UpdateActiveStateSql =
        "UPDATE Exchanges SET Active = @Active WHERE ExchangeSymbol = @ExchangeSymbol";

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ExchangePoint>> GetExchangesAsync()
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<ExchangePoint>(GetExchangesSql);
    }

    public async Task UpdateActiveStatesAsync(IEnumerable<ExchangePoint> exchanges)
    {
        var exchangeList = exchanges as IReadOnlyCollection<ExchangePoint> ?? [.. exchanges];
        if (exchangeList.Count == 0)
        {
            return;
        }

        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(UpdateActiveStateSql, exchangeList);
    }
}
