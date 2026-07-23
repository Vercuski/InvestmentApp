using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.TickerHandler.Queries;

public sealed record GetTickerListRequest() : IMediatRQueryRequest<List<Ticker>>;
internal sealed class GetTickerListHandler(IDbConnectionFactory dbConnectionFactory) : IMediatRQueryHandler<GetTickerListRequest, List<Ticker>>
{
    public Task<List<Ticker>> Handle(
        GetTickerListRequest request,
        CancellationToken cancellationToken)
    {
        var dbConnection = dbConnectionFactory.CreateReadConnection();
        List<Ticker>? response = [.. dbConnection.Query<Ticker>("SELECT tickerSymbol, Description, exchangeSymbol FROM dbo.ticker WHERE exchangeSymbol IN (SELECT exchangeSymbol FROM Exchanges WHERE active = 1) ORDER BY tickerSymbol")];

        if (response is null)
        {
            return Task.FromResult(new List<Ticker>());
        }

        return Task.FromResult(response);
    }
}
