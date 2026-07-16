using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Services;
using InvestmentApp.Domain.Entities;
using System.Data;
using System.Net;
using Z.Dapper.Plus;

namespace InvestmentApp.Application.Actions.StockDataHandler.Commands;

public sealed record DownloadSingleStockDataPointRequest(Ticker Ticker) : IMediatRCommandRequest<HttpStatusCode>;
internal class DownloadSingleStockDataPointHandler(IDbConnectionFactory dbConnectionFactory,
    IStockDataRepository stockDataRepository,
    IDataDownloadService dataDownloadService)
    : IMediatRCommandHandler<DownloadSingleStockDataPointRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(DownloadSingleStockDataPointRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            using IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            await stockDataRepository.DeleteStockDataByTicker(request.Ticker);
            List<StockData> completeList = [];
            List<StockData> Stock;
            (_, Stock) = await dataDownloadService.GetStock(request.Ticker);
            completeList.AddRange(Stock);
            await dbConnection.BulkInsertAsync<StockData>(completeList);
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