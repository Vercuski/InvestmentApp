using Dapper;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Infrastructure.Repositories;

public class StockDataRepository(IDbConnectionFactory connectionFactory) : IStockDataRepository
{
    private const string DeleteStockDataByTickerSql = "DELETE FROM StockData WHERE TickerSymbol = @TickerSymbol";
    private const string GetStockDataByTickerSymbolSql = @"SELECT
	[TickerSymbol],
	[Open],
	[High],
	[Low],
	[Close],
	[Volume],
	[Date]
FROM
	StockData
WHERE
	[TickerSymbol] = @TickerSymbol
ORDER BY
	[Date]";

    private const string GetLatestStockDataByTickerSymbolSql = @"SELECT TOP (1)
	[TickerSymbol],
	[Open],
	[High],
	[Low],
	[Close],
	[Volume],
	[Date]
FROM
	StockData
WHERE
	[TickerSymbol] = @TickerSymbol
ORDER BY
	[Date] DESC";

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<int> DeleteStockDataByTicker(Ticker ticker)
    {
        using var dbConnection = _connectionFactory.CreateWriteConnection();
        return await dbConnection.ExecuteAsync(DeleteStockDataByTickerSql, new { ticker.TickerSymbol });
    }

    public async Task<IEnumerable<StockData>> GetStockDataByTickerSymbolAsync(string tickerSymbol)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<StockData>(GetStockDataByTickerSymbolSql, new { TickerSymbol = tickerSymbol });
    }

    public async Task<StockData?> GetLatestStockDataByTickerSymbolAsync(string tickerSymbol)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QuerySingleOrDefaultAsync<StockData?>(GetLatestStockDataByTickerSymbolSql, new { TickerSymbol = tickerSymbol });
    }
}
