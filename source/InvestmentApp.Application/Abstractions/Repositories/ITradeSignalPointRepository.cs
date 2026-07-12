using InvestmentApp.Application.Contracts.Poco;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface ITradeSignalPointRepository
{
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestBuyTradeSignalPointsAsync();
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestSellTradeSignalPointsAsync();
}
