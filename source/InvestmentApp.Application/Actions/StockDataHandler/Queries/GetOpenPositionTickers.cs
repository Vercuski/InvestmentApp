using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.StockDataHandler.Queries;

/// <summary>
/// Resolves the distinct set of <see cref="Ticker"/>s for every position that has
/// not yet been sold (<see cref="PositionPoint.SellDate"/> is null).
/// </summary>
public sealed record GetOpenPositionTickersRequest() : IMediatRQueryRequest<List<Ticker>>;

internal sealed class GetOpenPositionTickersHandler(
    IPositionRepository positionRepository,
    ITickerRepository tickerRepository)
    : IMediatRQueryHandler<GetOpenPositionTickersRequest, List<Ticker>>
{
    public async Task<List<Ticker>> Handle(
        GetOpenPositionTickersRequest request,
        CancellationToken cancellationToken)
    {
        var positions = await positionRepository.GetAllPositionsAsync();

        var openTickerSymbols = positions
            .Where(position => position.SellDate is null)
            .Select(position => position.TickerSymbol)
            .Where(tickerSymbol => !string.IsNullOrWhiteSpace(tickerSymbol))
            .Select(tickerSymbol => tickerSymbol!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (openTickerSymbols.Count == 0)
        {
            return [];
        }

        var tickers = await tickerRepository.GetTickersBySymbolsAsync(openTickerSymbols);
        return [.. tickers];
    }
}
