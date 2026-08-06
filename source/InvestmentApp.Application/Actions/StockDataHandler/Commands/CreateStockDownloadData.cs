using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Services;
using InvestmentApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Net;
using Z.Dapper.Plus;
using Z.Dapper.Sql;

namespace InvestmentApp.Application.Actions.StockDataHandler.Commands;

public sealed record CreateStockDownloadRequest(List<Ticker> TickerList) : IMediatRCommandRequest<HttpStatusCode>;
internal class CreateStockDownloadHandler(IDbConnectionFactory dbConnectionFactory, IVpnService vpnService,
    IDataDownloadService dataDownloadService, ILogger<CreateStockDownloadHandler> logger)
    : IMediatRCommandHandler<CreateStockDownloadRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(CreateStockDownloadRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            using IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            dbConnection.TruncateTable<StockData>();
            const string vpnServer = "us";
            vpnService.ConnectToVPN(vpnServer);
            List<StockData> completeList = [];
            int count = 1;
            int max = request.TickerList.Count;
            foreach (var ticker in request.TickerList)
            {
                List<StockData> Stock;
                (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                if (statusCode == HttpStatusCode.TooManyRequests)
                {
                    vpnService.ConnectToVPN(vpnServer);
                    (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                }
                completeList.AddRange(Stock);
                logger.LogInformation("Finsihed {count} out of {max}", count++, max);
            }
            dbConnection.BulkInsert<StockData>(completeList);
            vpnService.DisconnectFromVPN();
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            logger.LogError("An error occurred: {StackTrace}", ex.StackTrace);
            throw;
        }
        return statusCode;
    }
}