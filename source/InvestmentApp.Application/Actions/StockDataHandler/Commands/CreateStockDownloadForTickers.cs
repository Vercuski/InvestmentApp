using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Application.Abstractions.Repositories;
using InvestmentApp.Application.Services;
using InvestmentApp.Domain.Entities;
using System.Data;
using System.Net;
using Z.Dapper.Plus;

namespace InvestmentApp.Application.Actions.StockDataHandler.Commands;

/// <summary>
/// Erases the existing StockData rows for the given tickers only (leaving all other
/// tickers' history untouched) and re-downloads fresh data in their place.
/// </summary>
public sealed record CreateStockDownloadForTickersRequest(List<Ticker> TickerList) : IMediatRCommandRequest<HttpStatusCode>;

internal class CreateStockDownloadForTickersHandler(
    IDbConnectionFactory dbConnectionFactory,
    IStockDataRepository stockDataRepository,
    IVpnService vpnService,
    IDataDownloadService dataDownloadService)
    : IMediatRCommandHandler<CreateStockDownloadForTickersRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(CreateStockDownloadForTickersRequest request
        , CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode = HttpStatusCode.OK;
        try
        {
            using IDbConnection dbConnection = dbConnectionFactory.CreateWriteConnection();
            const string vpnServer = "us";
            vpnService.ConnectToVPN(vpnServer);
            List<StockData> completeList = [];
            int count = 1;
            int max = request.TickerList.Count;
            foreach (var ticker in request.TickerList)
            {
                // Only erase this ticker's rows, not the entire StockData table -
                // other tickers' history must remain untouched.
                await stockDataRepository.DeleteStockDataByTicker(ticker);

                List<StockData> Stock;
                (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                if (statusCode == HttpStatusCode.TooManyRequests)
                {
                    vpnService.ConnectToVPN(vpnServer);
                    (statusCode, Stock) = await dataDownloadService.GetStock(ticker);
                }
                completeList.AddRange(Stock);
                Console.WriteLine($"Finished {count++} out of {max}");
            }
            await dbConnection.BulkInsertAsync<StockData>(completeList);
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
