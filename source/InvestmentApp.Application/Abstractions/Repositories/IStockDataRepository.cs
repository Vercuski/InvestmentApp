using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface IStockDataRepository
{
    Task<int> DeleteStockDataByTicker(Ticker ticker);

    /// <summary>
    /// Returns the full OHLC price history for a single ticker, ordered by date ascending.
    /// </summary>
    Task<IEnumerable<StockData>> GetStockDataByTickerSymbolAsync(string tickerSymbol);

    /// <summary>
    /// Returns the single most recent OHLC price bar for a ticker, or null if no
    /// price data has been recorded for it.
    /// </summary>
    Task<StockData?> GetLatestStockDataByTickerSymbolAsync(string tickerSymbol);
}
