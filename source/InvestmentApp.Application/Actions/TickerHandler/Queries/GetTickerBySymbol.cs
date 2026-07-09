using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.TickerHandler.Queries;

public sealed record GetTickerBySymbolRequest(string TickerSymbol) : IMediatRQueryRequest<Ticker?>;
internal sealed class GetTickerBySymbolHandler(IDbConnectionFactory dbConnectionFactory)
    : IMediatRQueryHandler<GetTickerBySymbolRequest, Ticker?>
{
    public Task<Ticker?> Handle(
        GetTickerBySymbolRequest request,
        CancellationToken cancellationToken)
    {
        var dbConnection = dbConnectionFactory.CreateReadConnection();
        Ticker? response = null;
        //dbContext.Ticker.AsNoTracking()
        //.Where(x => x.TickerSymbol == request.TickerSymbol)
        //.SingleOrDefault();

        return Task.FromResult(response);
    }
}
