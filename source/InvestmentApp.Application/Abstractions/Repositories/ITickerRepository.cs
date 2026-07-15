using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface ITickerRepository
{
    /// <summary>
    /// Returns the exchange codes (e.g. "NYSE", "NASDAQ") to download symbol lists
    /// for, read from the Exchanges table.
    /// </summary>
    Task<IEnumerable<ExchangePoint>> GetExchangeCodesAsync();

    /// <summary>
    /// Truncates the Ticker table and inserts the given tickers in its place.
    /// </summary>
    Task ReplaceAllAsync(IEnumerable<Ticker> tickers);
}
