using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;

namespace InvestmentApp.Infrastructure.Repositories;

public class StockDataRepository : IStockDataRepository
{
    private IDbConnectionFactory _connectionFactory;

    public StockDataRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
}
