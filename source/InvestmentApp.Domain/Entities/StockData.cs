namespace InvestmentApp.Domain.Entities;

/// <summary>
/// Represents an immutable daily OHLC (open-high-low-close) price bar for a single stock ticker.
/// All monetary values use <see cref="decimal"/> to avoid floating-point rounding error.
/// </summary>
public sealed record StockData
{
    public int TickerId { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public DateTime Date { get; set; }

    public StockData() { }

    /// <summary>
    /// Creates a validated <see cref="StockData"/> price bar.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the ticker is null/empty or the OHLC values are internally inconsistent.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a price or volume is negative.</exception>
    public StockData(
        int tickerId,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        DateTime date)
    {
        TickerId = tickerId;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        Date = date;
    }
}

