using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions.Repositories;

public interface IPositionRepository
{
    /// <summary>
    /// Returns every position on record, open and closed.
    /// </summary>
    Task<IEnumerable<PositionPoint>> GetAllPositionsAsync();

    /// <summary>
    /// Inserts a new position when <see cref="PositionPoint.PositionId"/> is 0,
    /// otherwise updates the existing row matched by <see cref="PositionPoint.PositionId"/>.
    /// Returns the saved position, with <see cref="PositionPoint.PositionId"/> populated.
    /// </summary>
    Task<PositionPoint> UpsertPositionAsync(PositionPoint position);
}
