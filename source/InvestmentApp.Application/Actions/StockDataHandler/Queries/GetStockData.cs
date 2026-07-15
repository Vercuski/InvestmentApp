using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.StockDataHandler.Queries;

public sealed record GetStockRequest(Ticker Ticker) : IMediatRQueryRequest<List<StockData>>;
internal sealed class GetStockHandler(IDbConnectionFactory dbConnectionFactory) : IMediatRQueryHandler<GetStockRequest, List<StockData>>
{
    public Task<List<StockData>> Handle(
        GetStockRequest request,
        CancellationToken cancellationToken)
    {
        using var readConnection = dbConnectionFactory.CreateReadConnection();
        List<StockData>? response = [];
        //[.. dbContext.Stock.AsNoTracking()
        //.Where(x => x.tickerSymbol == request.Ticker.tickerSymbol)
        //.OrderBy(x => x.Date)];

        if (response is null)
        {
            return Task.FromResult(new List<StockData>());
        }

        return Task.FromResult(response);
    }
}
