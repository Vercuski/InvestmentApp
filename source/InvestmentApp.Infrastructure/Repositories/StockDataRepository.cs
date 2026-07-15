using Dapper;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;
using Z.Dapper.Plus;

namespace InvestmentApp.Infrastructure.Repositories;

public class StockDataRepository(IDbConnectionFactory connectionFactory) : IStockDataRepository
{
    private const string DeleteStockDataByTickerSql = "DELETE FROM StockData WHERE TickerSymbol = @TickerSymbol";
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<int> DeleteStockDataByTicker(Ticker ticker)
    {
        using var dbConnection = _connectionFactory.CreateWriteConnection();
        return await dbConnection.ExecuteAsync(DeleteStockDataByTickerSql, new { ticker.TickerSymbol });
    }
}
