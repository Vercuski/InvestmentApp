using InvestmentApp.Application.Contracts.Poco;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface ITradeSignalPointRepository
{
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestBuyTradeSignalPointsAsync(int confidenceLevel = 100);
    Task<IEnumerable<TradeSignalPointPoco>> GetLatestSellTradeSignalPointsAsync(int confidenceLevel = 100);

    /// <summary>
    /// Returns every historical buy/sell signal for a single ticker (Hold points excluded),
    /// ordered by date ascending, for plotting as markers on a price chart.
    /// </summary>
    Task<IEnumerable<TradeSignalPointPoco>> GetTradeSignalPointsByTickerSymbolAsync(string tickerSymbol);
}
