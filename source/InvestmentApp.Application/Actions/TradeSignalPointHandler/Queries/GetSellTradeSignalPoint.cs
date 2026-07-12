using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Contracts.Poco;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.TickerHandler.Queries;

public sealed record GetLatestSellTradeSignalPointsRequest() : IMediatRQueryRequest<IEnumerable<TradeSignalPointPoco>>;
internal sealed class GetLatestSellTradeSignalPointsHandler(ITradeSignalPointRepository tradeSignalRepository)
    : IMediatRQueryHandler<GetLatestSellTradeSignalPointsRequest, IEnumerable<TradeSignalPointPoco>>
{
    public async Task<IEnumerable<TradeSignalPointPoco>> Handle(
        GetLatestSellTradeSignalPointsRequest request,
        CancellationToken cancellationToken)
    {
        return await tradeSignalRepository.GetLatestSellTradeSignalPointsAsync();
    }
}
