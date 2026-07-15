using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Abstractions;

public interface IEodDataScraperService
{
    /// <summary>
    /// Logs into eoddata.com and downloads the symbol list for each of the given
    /// exchange codes (e.g. "NYSE", "NASDAQ"), returning the combined, de-duplicated
    /// set of tickers.
    /// </summary>
    Task<List<Ticker>> DownloadSymbolsAsync(IEnumerable<ExchangePoint> exchangeCodes, CancellationToken cancellationToken);
}
