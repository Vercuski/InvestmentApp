using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Aroon indicator (Aroon Up, Aroon Down, and the Aroon Oscillator) for a
/// chronological series of <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// Within each trailing window of <see cref="Period"/> + 1 bars (the current bar plus
/// <see cref="Period"/> prior bars), Aroon Up measures how recently the window's highest
/// high occurred and Aroon Down measures how recently its lowest low occurred, each scaled
/// so a value of 100 means "at the current bar" and 0 means "at the oldest bar in the
/// window." Unlike <see cref="AdxCalculator"/>, which measures trend strength without
/// direction, Aroon Up and Aroon Down together indicate both strength and direction: one
/// running high while the other runs low signals a strong trend in that direction, while
/// both sitting in the middle signals a consolidating, directionless market.
/// </remarks>
public sealed class AroonCalculator
{
    public int Period { get; }
    public decimal StrongTrendThreshold { get; }
    public decimal WeakTrendThreshold { get; }

    /// <summary>
    /// Creates a calculator with the given lookback period and trend classification
    /// thresholds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not positive, or when a threshold falls
    /// outside the valid 0-100 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="strongTrendThreshold"/> is not greater than <paramref name="weakTrendThreshold"/>.
    /// </exception>
    public AroonCalculator(
        int period = 25,
        decimal strongTrendThreshold = 70m,
        decimal weakTrendThreshold = 30m)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (strongTrendThreshold > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(strongTrendThreshold), strongTrendThreshold, "Strong trend threshold cannot exceed 100.");
        }

        if (weakTrendThreshold < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(weakTrendThreshold), weakTrendThreshold, "Weak trend threshold cannot be negative.");
        }

        if (strongTrendThreshold <= weakTrendThreshold)
        {
            throw new ArgumentException($"Strong trend threshold ({strongTrendThreshold}) must be greater than weak trend threshold ({weakTrendThreshold}).", nameof(strongTrendThreshold));
        }

        Period = period;
        StrongTrendThreshold = strongTrendThreshold;
        WeakTrendThreshold = weakTrendThreshold;
    }

    /// <summary>
    /// Computes the Aroon series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one Aroon value.
    /// </exception>
    public IReadOnlyList<AroonPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Aroon series; {bars.Count} were provided.",
                nameof(series));
        }

        var points = new List<AroonPoint>(bars.Count - Period);

        for (int i = Period; i < bars.Count; i++)
        {
            int windowStart = i - Period;

            int highestHighIndex = windowStart;
            int lowestLowIndex = windowStart;
            for (int j = windowStart + 1; j <= i; j++)
            {
                if (bars[j].High >= bars[highestHighIndex].High)
                {
                    highestHighIndex = j;
                }
                if (bars[j].Low <= bars[lowestLowIndex].Low)
                {
                    lowestLowIndex = j;
                }
            }

            int periodsSinceHigh = i - highestHighIndex;
            int periodsSinceLow = i - lowestLowIndex;

            decimal aroonUp = 100m * (Period - periodsSinceHigh) / Period;
            decimal aroonDown = 100m * (Period - periodsSinceLow) / Period;
            decimal oscillator = aroonUp - aroonDown;

            var trend = AroonTrend.Neutral;
            if (aroonUp >= StrongTrendThreshold && aroonDown <= WeakTrendThreshold)
            {
                trend = AroonTrend.Uptrend;
            }
            else if (aroonDown >= StrongTrendThreshold && aroonUp <= WeakTrendThreshold)
            {
                trend = AroonTrend.Downtrend;
            }

            points.Add(new AroonPoint(bars[i].TickerSymbol, bars[i].Date, aroonUp, aroonDown, oscillator, trend));
        }

        return points;
    }
}
