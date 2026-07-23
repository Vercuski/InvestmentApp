using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.StockDataHandler.Queries;

public sealed record GetLatestStockDataByTickerSymbolRequest(string TickerSymbol) : IMediatRQueryRequest<StockData?>;
internal sealed class GetLatestStockDataByTickerSymbolHandler(IStockDataRepository stockDataRepository)
    : IMediatRQueryHandler<GetLatestStockDataByTickerSymbolRequest, StockData?>
{
    public async Task<StockData?> Handle(
        GetLatestStockDataByTickerSymbolRequest request,
        CancellationToken cancellationToken)
    {
        return await stockDataRepository.GetLatestStockDataByTickerSymbolAsync(request.TickerSymbol);
    }
}
