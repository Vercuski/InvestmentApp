using Dapper;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;
using Z.Dapper.Plus;
using Z.Dapper.Sql;

namespace InvestmentApp.Infrastructure.Repositories;

public class TickerRepository(IDbConnectionFactory connectionFactory) : ITickerRepository
{
    // Assumes an Exchanges table with an ExchangeSymbol column (e.g. NYSE, NASDAQ).
    // Adjust the table/column names here if your schema differs.
    private const string GetExchangeCodesSql = "SELECT ExchangeSymbol, ExchangeDescription, Active FROM Exchanges ORDER BY ExchangeSymbol";

    private const string TruncateTickerSql = "TRUNCATE TABLE Ticker";

    private const string InsertTickerSql =
        "INSERT INTO Ticker (TickerSymbol, Description, ExchangeSymbol) VALUES (@TickerSymbol, @Description, @ExchangeSymbol)";

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ExchangePoint>> GetExchangeCodesAsync()
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<ExchangePoint>(GetExchangeCodesSql);
    }

    public async Task ReplaceAllAsync(IEnumerable<Ticker> tickers)
    {
        var tickerList = tickers as IReadOnlyCollection<Ticker> ?? [.. tickers];

        using var connection = _connectionFactory.CreateWriteConnection();
        connection.TruncateTable<Ticker>();

        if (tickerList.Count > 0)
        {
            await connection.BulkInsertAsync<Ticker>(tickerList);
        }
    }
}
