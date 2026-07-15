using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// Represents an immutable daily OHLC (open-high-low-close) price bar for a single stock ticker.
/// All monetary values use <see cref="decimal"/> to avoid floating-point rounding error.
/// </summary>
public sealed record StockData : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public DateTime Date { get; set; }

    public StockData() { }

    /// <summary>
    /// Creates a <see cref="StockData"/> price bar.
    /// </summary>
    public StockData(
        string? tickerSymbol,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        DateTime date)
    {
        TickerSymbol = tickerSymbol;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        Date = date;
    }
}

