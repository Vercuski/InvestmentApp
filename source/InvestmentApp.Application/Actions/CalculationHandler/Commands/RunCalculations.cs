using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Calculators;
using InvestmentApp.Domain.Entities;
using System.Data;
using System.Net;
using Z.Dapper.Plus;
using Z.Dapper.Sql;

namespace InvestmentApp.Application.Actions.CalculationHandler.Commands;

public sealed record RunCalculationRequest() : IMediatRCommandRequest<HttpStatusCode>;
internal class RunCalculationHandler(IDbConnectionFactory dbConnectionFactory,
    MacdCalculator macdCalculator,
    RsiCalculator rsiCalculator,
    BollingerBandsCalculator bollingerBandsCalculator,
    ObvCalculator obvCalculator,
    StochasticCalculator stochasticCalculator,
    AtrCalculator atrCalculator,
    AdxCalculator adxCalculator,
    CciCalculator cciCalculator,
    ChaikinMoneyFlowCalculator chaikinMoneyFlowCalculator,
    KeltnerChannelsCalculator keltnerChannelsCalculator,
    //MovingAverageCrossoverCalculator movingAverageCrossoverCalculator,
    SignalAggregator signalAggregator)
    : IMediatRCommandHandler<RunCalculationRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(RunCalculationRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            using IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            dbConnection.TruncateTable<TradeSignalPoint>();
            var tickerList = dbConnection.Query<Ticker>("SELECT tickerSymbol FROM Ticker WHERE exchangeSymbol IN (SELECT exchangeSymbol FROM Exchanges WHERE active = 1) ORDER BY tickerSymbol").ToList();
            List<TradeSignalPoint> signalAggregatorCalculation = [];
            foreach (Ticker ticker in tickerList)
            {
                var stockDataList = dbConnection.Query<StockData>($"SELECT [tickerSymbol], [open], [high], [low], [close], [volume], [date] FROM StockData WHERE [tickerSymbol] = '{ticker.TickerSymbol}' ORDER BY [date]").ToList();
                if (stockDataList.Count < 50)
                {
                    Console.WriteLine($"No stock data found for ticker: {ticker.TickerSymbol}");
                    continue;
                }
                var macdCalculation = macdCalculator.Calculate(stockDataList);
                var rsiCalculation = rsiCalculator.Calculate(stockDataList);
                var bollingerBandsCalculation = bollingerBandsCalculator.Calculate(stockDataList);
                var obvCalculation = obvCalculator.Calculate(stockDataList);
                var stochasticCalculation = stochasticCalculator.Calculate(stockDataList);
                var atrCalculation = atrCalculator.Calculate(stockDataList);
                var adxCalculation = adxCalculator.Calculate(stockDataList);
                var cciCalculation = cciCalculator.Calculate(stockDataList);
                var chaikinMoneyFlowCalculation = chaikinMoneyFlowCalculator.Calculate(stockDataList);
                var keltnerChannelsCalculation = keltnerChannelsCalculator.Calculate(stockDataList);
                //var movingAverageCrossoverCalculation = movingAverageCrossoverCalculator.Calculate(stockDataList);
                signalAggregatorCalculation.AddRange(signalAggregator.Calculate(
                    stockDataList,
                    macdCalculation,
                    rsiCalculation,
                    bollingerBandsCalculation,
                    adxCalculation,
                    obvCalculation,
                    atrCalculation));
            }
            dbConnection.BulkInsert<TradeSignalPoint>(signalAggregatorCalculation);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"An error occurred: {ex.StackTrace}");
            throw;
        }
        return statusCode;
    }
}