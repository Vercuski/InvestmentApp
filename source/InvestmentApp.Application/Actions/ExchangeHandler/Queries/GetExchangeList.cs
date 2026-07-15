using InvestmentApp.Application.Abstractions;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.ExchangeHandler.Queries;

public sealed record GetExchangeListRequest() : IMediatRQueryRequest<IEnumerable<ExchangePoint>>;

internal sealed class GetExchangeListHandler(IExchangeRepository exchangeRepository)
    : IMediatRQueryHandler<GetExchangeListRequest, IEnumerable<ExchangePoint>>
{
    public async Task<IEnumerable<ExchangePoint>> Handle(
        GetExchangeListRequest request,
        CancellationToken cancellationToken)
    {
        return await exchangeRepository.GetExchangesAsync();
    }
}
