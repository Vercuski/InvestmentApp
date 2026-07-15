using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.TickerHandler.Queries;

public sealed record GetTickerBySymbolRequest(string TickerSymbol) : IMediatRQueryRequest<Ticker?>;
internal sealed class GetTickerBySymbolHandler(ITickerRepository tickerRepository)
    : IMediatRQueryHandler<GetTickerBySymbolRequest, Ticker?>
{
    public async Task<Ticker?> Handle(
        GetTickerBySymbolRequest request,
        CancellationToken cancellationToken)
    {
        return await tickerRepository.GetTickerBySymbolAsync(request.TickerSymbol);
    }
}
