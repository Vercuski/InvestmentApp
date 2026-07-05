namespace InvestmentApp.Domain.Entities;

/// <summary>
/// Represents an immutable daily OHLC (open-high-low-close) price bar for a single stock ticker.
/// All monetary values use <see cref="decimal"/> to avoid floating-point rounding error.
/// </summary>
public sealed record Stock
{
    public string Ticker { get; }
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public decimal AdjustedClose { get; }
    public long Volume { get; }
    public DateOnly PriceDate { get; }

    /// <summary>
    /// Creates a validated <see cref="Stock"/> price bar.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the ticker is null/empty or the OHLC values are internally inconsistent.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a price or volume is negative.</exception>
    public Stock(
        string ticker,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal adjustedClose,
        long volume,
        DateOnly priceDate)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            throw new ArgumentException("Ticker cannot be null or empty.", nameof(ticker));
        }

        if (open < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(open), open, "Open price cannot be negative.");
        }

        if (high < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(high), high, "High price cannot be negative.");
        }

        if (low < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(low), low, "Low price cannot be negative.");
        }

        if (close < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(close), close, "Close price cannot be negative.");
        }

        if (adjustedClose < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustedClose), adjustedClose, "Adjusted close price cannot be negative.");
        }

        if (volume < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "Volume cannot be negative.");
        }

        if (high < low)
        {
            throw new ArgumentException($"High price ({high}) cannot be less than low price ({low}).");
        }

        if (high < open)
        {
            throw new ArgumentException($"High price ({high}) cannot be less than open price ({open}).");
        }

        if (high < close)
        {
            throw new ArgumentException($"High price ({high}) cannot be less than close price ({close}).");
        }

        if (low > open)
        {
            throw new ArgumentException($"Low price ({low}) cannot be greater than open price ({open}).");
        }

        if (low > close)
        {
            throw new ArgumentException($"Low price ({low}) cannot be greater than close price ({close}).");
        }

        Ticker = ticker.Trim().ToUpperInvariant();
        Open = open;
        High = high;
        Low = low;
        Close = close;
        AdjustedClose = adjustedClose;
        Volume = volume;
        PriceDate = priceDate;
    }
}

