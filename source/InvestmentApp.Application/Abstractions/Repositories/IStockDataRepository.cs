using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface IStockDataRepository
{
    Task<int> DeleteStockDataByTicker(Ticker ticker);
}
