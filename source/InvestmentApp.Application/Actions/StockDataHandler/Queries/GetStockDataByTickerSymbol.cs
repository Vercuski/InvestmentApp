using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.StockDataHandler.Queries;

public sealed record GetStockDataByTickerSymbolRequest(string TickerSymbol) : IMediatRQueryRequest<IEnumerable<StockData>>;
internal sealed class GetStockDataByTickerSymbolHandler(IStockDataRepository stockDataRepository)
    : IMediatRQueryHandler<GetStockDataByTickerSymbolRequest, IEnumerable<StockData>>
{
    public async Task<IEnumerable<StockData>> Handle(
        GetStockDataByTickerSymbolRequest request,
        CancellationToken cancellationToken)
    {
        return await stockDataRepository.GetStockDataByTickerSymbolAsync(request.TickerSymbol);
    }
}
