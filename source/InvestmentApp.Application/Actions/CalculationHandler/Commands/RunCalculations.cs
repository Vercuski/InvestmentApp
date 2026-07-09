using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Calculators;
using InvestmentApp.Domain.Entities;
using System.Data;
using System.Net;

namespace InvestmentApp.Application.Actions.CalculationHandler.Commands;

public sealed record RunCalculationRequest() : IMediatRCommandRequest<HttpStatusCode>;
internal class RunCalculationHandler(IDbConnectionFactory dbConnectionFactory,
    MacdCalculator macdCalculator,
    RsiCalculator rsiCalculator,
    BollingerBandsCalculator bollingerBandsCalculator,
    ObvCalculator obvCalculator)
    : IMediatRCommandHandler<RunCalculationRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(RunCalculationRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            var tickerList = dbConnection.Query<Ticker>("SELECT tickerId, tickerSymbol FROM Ticker ORDER BY tickerSymbol").ToList();
            foreach (Ticker ticker in tickerList)
            {
                var stockDataList = dbConnection.Query<StockData>($"SELECT [tickerId], [open], [high], [low], [close], [volume], [date] FROM StockData WHERE [tickerId] = {ticker.TickerId} ORDER BY [date]").ToList();
                var macdCalculation = macdCalculator.Calculate(stockDataList);
                var rsiCalculation = rsiCalculator.Calculate(stockDataList);
                var bollingerBandsCalculation = bollingerBandsCalculator.Calculate(stockDataList);
                var obvCalculation = obvCalculator.Calculate(stockDataList);
            }
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