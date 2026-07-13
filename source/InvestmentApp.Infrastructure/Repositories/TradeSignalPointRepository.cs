using Dapper;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Contracts.Poco;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Infrastructure.Repositories;

public class TradeSignalPointRepository : ITradeSignalPointRepository
{
	private const string GetLatesTradeSignalPointsSQL = @"SELECT
	T.tickerSymbol,
	SD.[close],
	TSP.Action,
	TSP.confidence,
	TSP.Regime,
	TSP.StopLossPrice,
	SD.[Date],
	TSP.PriceDate
FROM
	TradeSignalPoint TSP
	INNER JOIN stockData SD
		ON TSP.tickerId = SD.tickerId
		AND TSP.priceDate = SD.[date]
	INNER JOIN ticker T
		ON T.tickerId = TSP.tickerId
WHERE
	[action] <> 'Hold'
	AND confidence = @confidenceLevel
	AND SD.[close] < 100
	AND SD.date in (SELECT MAX(PriceDate) from TradeSignalPoint)
	AND [action] = @actionType
ORDER by
	T.tickerSymbol,
	SD.[Date]";

	private IDbConnectionFactory _connectionFactory;

	public TradeSignalPointRepository(IDbConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task<IEnumerable<TradeSignalPointPoco>> GetLatestBuyTradeSignalPointsAsync([FromQuery] int confidenceLevel = 100)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<TradeSignalPointPoco>(GetLatesTradeSignalPointsSQL, new { actionType = "buy", confidenceLevel = confidenceLevel/100.0 } );
    }

    public async Task<IEnumerable<TradeSignalPointPoco>> GetLatestSellTradeSignalPointsAsync([FromQuery] int confidenceLevel = 100)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<TradeSignalPointPoco>(GetLatesTradeSignalPointsSQL, new { actionType = "sell", confidenceLevel = confidenceLevel / 100.0 });
    }
}
