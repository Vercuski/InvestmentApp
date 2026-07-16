using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.PositionHandler.Queries;

public sealed record GetPositionListRequest() : IMediatRQueryRequest<IEnumerable<PositionPoint>>;

internal sealed class GetPositionListHandler(IPositionRepository positionRepository)
    : IMediatRQueryHandler<GetPositionListRequest, IEnumerable<PositionPoint>>
{
    public async Task<IEnumerable<PositionPoint>> Handle(
        GetPositionListRequest request,
        CancellationToken cancellationToken)
    {
        return await positionRepository.GetAllPositionsAsync();
    }
}
