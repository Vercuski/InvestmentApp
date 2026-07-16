using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.PositionHandler.Commands;

public sealed record UpsertPositionRequest(PositionPoint Position) : IMediatRCommandRequest<PositionPoint>;

internal sealed class UpsertPositionHandler(IPositionRepository positionRepository)
    : IMediatRCommandHandler<UpsertPositionRequest, PositionPoint>
{
    public async Task<PositionPoint> Handle(
        UpsertPositionRequest request,
        CancellationToken cancellationToken)
    {
        return await positionRepository.UpsertPositionAsync(request.Position);
    }
}
