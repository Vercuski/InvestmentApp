using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Contracts.Poco;

namespace InvestmentApp.Application.Actions.TradeSignalPointHandler.Queries;

public sealed record GetTradeSignalPointsByTickerRequest(string TickerSymbol) : IMediatRQueryRequest<IEnumerable<TradeSignalPointPoco>>;
internal sealed class GetTradeSignalPointsByTickerHandler(ITradeSignalPointRepository tradeSignalRepository)
    : IMediatRQueryHandler<GetTradeSignalPointsByTickerRequest, IEnumerable<TradeSignalPointPoco>>
{
    public async Task<IEnumerable<TradeSignalPointPoco>> Handle(
        GetTradeSignalPointsByTickerRequest request,
        CancellationToken cancellationToken)
    {
        return await tradeSignalRepository.GetTradeSignalPointsByTickerSymbolAsync(request.TickerSymbol);
    }
}
