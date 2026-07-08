using InvestmentApp.Domain.Entities;
using System.Net;

namespace InvestmentApp.Application.Services;

public interface IDataDownloadService
{
    Task<(HttpStatusCode statusCode, List<StockData> Stock)> GetStock(Ticker ticker);
}
