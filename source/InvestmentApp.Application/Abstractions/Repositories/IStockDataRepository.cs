using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface IStockDataRepository
{
    Task<int> DeleteStockDataByTicker(Ticker ticker);

    /// <summary>
    /// Returns the full OHLC price history for a single ticker, ordered by date ascending.
    /// </summary>
    Task<IEnumerable<StockData>> GetStockDataByTickerSymbolAsync(string tickerSymbol);
}
