using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Relative Strength Index (RSI) momentum oscillator for a
/// chronological series of <see cref="Stock"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// RSI = 100 - (100 / (1 + RS)), where RS is the ratio of average gain to average
/// loss over the lookback period. Average gain/loss is seeded with a simple average
/// of the first <see cref="Period"/> changes, then smoothed using Wilder's method
/// (the standard approach for RSI), which weights the current period more heavily
/// as the series progresses.
/// </remarks>
public sealed class RsiCalculator
{
    public int Period { get; }
    public decimal OverboughtThreshold { get; }
    public decimal OversoldThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and zone thresholds.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="Stock"/> feeds the calculation.
    /// Defaults to <see cref="Stock.Close"/>; pass <c>s => s.AdjustedClose</c> instead
    /// to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not positive, or when a threshold falls
    /// outside the valid 0-100 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="overboughtThreshold"/> is not greater than <paramref name="oversoldThreshold"/>.
    /// </exception>
    public RsiCalculator(
        int period = 14,
        decimal overboughtThreshold = 70m,
        decimal oversoldThreshold = 30m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (overboughtThreshold > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(overboughtThreshold), overboughtThreshold, "Overbought threshold cannot exceed 100.");
        }

        if (oversoldThreshold < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(oversoldThreshold), oversoldThreshold, "Oversold threshold cannot be negative.");
        }

        if (overboughtThreshold <= oversoldThreshold)
        {
            throw new ArgumentException($"Overbought threshold ({overboughtThreshold}) must be greater than oversold threshold ({oversoldThreshold}).", nameof(overboughtThreshold));
        }

        Period = period;
        OverboughtThreshold = overboughtThreshold;
        OversoldThreshold = oversoldThreshold;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the RSI series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="Stock.PriceDate"/> before computing,
    /// so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one RSI value.
    /// </exception>
    public IReadOnlyList<RsiPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        int minimumBars = Period + 1;
        if (bars.Count < minimumBars)
        {
            throw new ArgumentException(
                $"At least {minimumBars} price bars are required to compute a " +
                $"{Period}-period RSI series; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();

        // One fewer change than prices: change[i] = prices[i + 1] - prices[i].
        var gains = new decimal[prices.Count - 1];
        var losses = new decimal[prices.Count - 1];
        for (int i = 1; i < prices.Count; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            gains[i - 1] = change > 0 ? change : 0m;
            losses[i - 1] = change < 0 ? -change : 0m;
        }

        decimal seedGainSum = 0m;
        decimal seedLossSum = 0m;
        for (int i = 0; i < Period; i++)
        {
            seedGainSum += gains[i];
            seedLossSum += losses[i];
        }

        decimal averageGain = seedGainSum / Period;
        decimal averageLoss = seedLossSum / Period;

        var points = new List<RsiPoint>(prices.Count - Period)
        {
            BuildPoint(bars[Period].TickerSymbol,  bars[Period].Date, averageGain, averageLoss)
        };

        for (int i = Period; i < gains.Length; i++)
        {
            // Wilder's smoothing: weight the running average by (Period - 1) parts
            // history to 1 part the newest gain/loss.
            averageGain = (averageGain * (Period - 1) + gains[i]) / Period;
            averageLoss = (averageLoss * (Period - 1) + losses[i]) / Period;

            int barIndex = i + 1;
            points.Add(BuildPoint(bars[barIndex].TickerSymbol, bars[barIndex].Date, averageGain, averageLoss));
        }

        return points;
    }

    private RsiPoint BuildPoint(string? tickerSymbol, DateTime priceDate, decimal averageGain, decimal averageLoss)
    {
        decimal rsi;
        if (averageLoss == 0m)
        {
            // No losses in the window: maximally strong (or perfectly flat if there
            // were no gains either, which we treat as neutral rather than divide by zero).
            rsi = averageGain == 0m ? 50m : 100m;
        }
        else
        {
            decimal relativeStrength = averageGain / averageLoss;
            rsi = 100m - 100m / (1m + relativeStrength);
        }

        var zone = RsiZone.Neutral;
        if (rsi >= OverboughtThreshold)
        {
            zone = RsiZone.Overbought;
        }
        else if (rsi <= OversoldThreshold)
        {
            zone = RsiZone.Oversold;
        }

        return new RsiPoint(tickerSymbol, priceDate, rsi, zone);
    }
}
