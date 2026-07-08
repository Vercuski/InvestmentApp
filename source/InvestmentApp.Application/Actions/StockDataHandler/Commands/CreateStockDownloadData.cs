using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Services;
using InvestmentApp.Domain.Entities;
using System.Data;
using System.Net;
using Z.Dapper.Plus;

namespace InvestmentApp.Application.Actions.StockDataHandler.Commands;

public sealed record CreateStockDownloadRequest(List<Ticker> TickerList) : IMediatRCommandRequest<HttpStatusCode>;
internal class CreateStockDownloadHandler(IDbConnectionFactory dbConnectionFactory, IVPNService vpnService,
    IDataDownloadService dataDownloadService)
    : IMediatRCommandHandler<CreateStockDownloadRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(CreateStockDownloadRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            const string vpnServer = "us";
            vpnService.ConnectToVPN(vpnServer);
            List<StockData> completeList = [];
            int count = 1;
            int max = request.TickerList.Count;
            foreach (var ticker in request.TickerList)
            //foreach (var ticker in request.TickerList.Take(10))
            {
                List<StockData> Stock;
                (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                if (statusCode == HttpStatusCode.TooManyRequests)
                {
                    vpnService.ConnectToVPN(vpnServer);
                    (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                }
                completeList.AddRange(Stock);
                Console.WriteLine($"Finsihed {count++} out of {max}");
            }
            dbConnection.BulkInsert<StockData>(completeList);
            vpnService.DisconnectFromVPN();
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