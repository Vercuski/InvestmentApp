using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface ITickerRepository
{
    /// <summary>
    /// Returns the exchange codes (e.g. "NYSE", "NASDAQ") to download symbol lists
    /// for, read from the Exchanges table.
    /// </summary>
    Task<IEnumerable<ExchangePoint>> GetExchangeCodesAsync();

    Task<Ticker?> GetTickerBySymbolAsync(string tickerSymbol);

    /// <summary>
    /// Returns the tickers matching any of the given symbols. Symbols with no
    /// matching row are simply omitted from the result.
    /// </summary>
    Task<IEnumerable<Ticker>> GetTickersBySymbolsAsync(IEnumerable<string> tickerSymbols);

    /// <summary>
    /// Truncates the Ticker table and inserts the given tickers in its place.
    /// </summary>
    Task ReplaceAllAsync(IEnumerable<Ticker> tickers);
}
