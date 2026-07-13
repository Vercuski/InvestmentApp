using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Contracts.Poco;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.TickerHandler.Queries;

public sealed record GetLatestBuyTradeSignalPointsRequest(int ConfidenceLevel = 100) : IMediatRQueryRequest<IEnumerable<TradeSignalPointPoco>>;
internal sealed class GetLatestBuyTradeSignalPointsHandler(ITradeSignalPointRepository tradeSignalRepository)
    : IMediatRQueryHandler<GetLatestBuyTradeSignalPointsRequest, IEnumerable<TradeSignalPointPoco>>
{
    public async Task<IEnumerable<TradeSignalPointPoco>> Handle(
        GetLatestBuyTradeSignalPointsRequest request,
        CancellationToken cancellationToken)
    {
        return await tradeSignalRepository.GetLatestBuyTradeSignalPointsAsync(request.ConfidenceLevel);
    }
}
