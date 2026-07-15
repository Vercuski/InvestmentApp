using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;

namespace InvestmentApp.Infrastructure.Repositories;

public class StockDataRepository(IDbConnectionFactory connectionFactory) : IStockDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
}
