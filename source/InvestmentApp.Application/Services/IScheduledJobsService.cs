namespace InvestmentApp.Application.Services;

public interface IScheduledJobsService
{
    Task RunDailyDownloadAndCalculationAsync();
    Task DownloadOpenPositionStockDataAsync();
}