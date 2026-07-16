using Dapper;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Infrastructure.Repositories;

public class PositionRepository(IDbConnectionFactory connectionFactory) : IPositionRepository
{
    private const string GetAllPositionsSql = @"SELECT
    positionId,
    tickerSymbol,
    exchangeSymbol,
    regime,
    confidence,
    purchasePrice,
    numberOfShares,
    purchaseDate,
    sellPrice,
    sellDate
FROM [Position]
ORDER BY purchaseDate DESC, positionId DESC";

    private const string InsertPositionSql = @"INSERT INTO [Position]
(
    tickerSymbol,
    exchangeSymbol,
    regime,
    confidence,
    purchasePrice,
    numberOfShares,
    purchaseDate,
    sellPrice,
    sellDate
)
OUTPUT INSERTED.positionId
VALUES
(
    @TickerSymbol,
    @ExchangeSymbol,
    @Regime,
    @Confidence,
    @PurchasePrice,
    @NumberOfShares,
    @PurchaseDate,
    @SellPrice,
    @SellDate
)";

    private const string UpdatePositionSql = @"UPDATE [Position]
SET
    tickerSymbol = @TickerSymbol,
    exchangeSymbol = @ExchangeSymbol,
    regime = @Regime,
    confidence = @Confidence,
    purchasePrice = @PurchasePrice,
    numberOfShares = @NumberOfShares,
    purchaseDate = @PurchaseDate,
    sellPrice = @SellPrice,
    sellDate = @SellDate
WHERE
    positionId = @PositionId";

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<PositionPoint>> GetAllPositionsAsync()
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return (await connection.QueryAsync<PositionPoint>(GetAllPositionsSql)).OrderBy(x=>x.TickerSymbol);
    }

    public async Task<PositionPoint> UpsertPositionAsync(PositionPoint position)
    {
        using var connection = _connectionFactory.CreateWriteConnection();

        // Regime is a VARCHAR column: convert explicitly rather than relying on Dapper
        // to send the enum's underlying int for a write. Dapper does handle the reverse
        // (string column -> enum property) automatically on read.
        var parameters = new
        {
            position.PositionId,
            position.TickerSymbol,
            position.ExchangeSymbol,
            Regime = position.Regime.ToString(),
            position.Confidence,
            position.PurchasePrice,
            position.NumberOfShares,
            position.PurchaseDate,
            position.SellPrice,
            position.SellDate
        };

        if (position.PositionId == 0)
        {
            var newPositionId = await connection.ExecuteScalarAsync<int>(InsertPositionSql, parameters);
            position.PositionId = newPositionId;
        }
        else
        {
            await connection.ExecuteAsync(UpdatePositionSql, parameters);
        }

        return position;
    }
}
