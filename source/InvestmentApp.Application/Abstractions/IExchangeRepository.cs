using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions;

public interface IExchangeRepository
{
    /// <summary>
    /// Returns every row in the Exchanges table, active or not.
    /// </summary>
    Task<IEnumerable<ExchangePoint>> GetExchangesAsync();

    /// <summary>
    /// Updates the Active flag for each given exchange, matched by ExchangeSymbol.
    /// </summary>
    Task UpdateActiveStatesAsync(IEnumerable<ExchangePoint> exchanges);
}
