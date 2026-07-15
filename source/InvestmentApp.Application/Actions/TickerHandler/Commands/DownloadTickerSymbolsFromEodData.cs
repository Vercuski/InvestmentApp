using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.Repositories;
using System.Net;

namespace InvestmentApp.Application.Actions.TickerHandler.Commands;

public sealed record DownloadTickerSymbolsRequest() : IMediatRCommandRequest<HttpStatusCode>;

internal sealed class DownloadTickerSymbolsHandler(
    ITickerRepository tickerRepository,
    IEodDataScraperService eodDataScraperService)
    : IMediatRCommandHandler<DownloadTickerSymbolsRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(DownloadTickerSymbolsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var exchangeCodes = (await tickerRepository.GetExchangeCodesAsync()).ToList();
            if (exchangeCodes.Count == 0)
            {
                Console.WriteLine("No exchange codes found in the Exchanges table.");
                return HttpStatusCode.NoContent;
            }

            var tickers = await eodDataScraperService.DownloadSymbolsAsync(exchangeCodes, cancellationToken);
            if (tickers.Count == 0)
            {
                Console.WriteLine("No ticker symbols were downloaded from EODData.");
                return HttpStatusCode.NoContent;
            }

            await tickerRepository.ReplaceAllAsync(tickers);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.StackTrace}");
            throw;
        }

        return HttpStatusCode.OK;
    }
}
