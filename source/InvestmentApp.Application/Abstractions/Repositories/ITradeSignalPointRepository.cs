using InvestmentApp.Application.Contracts.Poco;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface ITradeSignalPointRepository
{
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestBuyTradeSignalPointsAsync(int confidenceLevel = 100);
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestSellTradeSignalPointsAsync(int confidenceLevel = 100);
}
