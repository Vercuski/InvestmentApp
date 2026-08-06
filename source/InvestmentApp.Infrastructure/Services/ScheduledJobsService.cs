using InvestmentApp.Application.Services;

namespace InvestmentApp.Infrastructure.Services;

public sealed class ScheduledJobsService(IHttpClientFactory httpClientFactory) : IScheduledJobsService
{
    public async Task RunDailyDownloadAndCalculationAsync()
    {
        var client = httpClientFactory.CreateClient("localhost");

        var downloadResponse = await client.PostAsync("api/StockData/Download/All", content: null);
        downloadResponse.EnsureSuccessStatusCode();

        var calculationResponse = await client.PostAsync("api/Calculation", content: null);
        calculationResponse.EnsureSuccessStatusCode();
    }

    public async Task DownloadOpenPositionStockDataAsync()
    {
        var client = httpClientFactory.CreateClient("localhost");

        var response = await client.PostAsync("api/StockData/Download/OpenPositions", content: null);
        response.EnsureSuccessStatusCode();
    }
}