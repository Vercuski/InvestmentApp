using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Actions;

/// <summary>
/// Computes the Moving Average Convergence Divergence (MACD) indicator for a
/// chronological series of <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// MACD = EMA(fastPeriod) - EMA(slowPeriod), computed on a chosen price field.
/// Signal = EMA(signalPeriod) of the MACD line. Histogram = MACD - Signal.
/// Each EMA is seeded with a simple moving average of its first N values, which is
/// the standard approach for financial EMAs and avoids biasing early values toward
/// the very first price in the series.
/// </remarks>
public sealed class MacdCalculator
{
    public int FastPeriod { get; }
    public int SlowPeriod { get; }
    public int SignalPeriod { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given EMA periods.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> feeds the calculation.
    /// Defaults to <see cref="StockData.Close"/>; pass <c>s => s.AdjustedClose</c> instead
    /// to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fastPeriod"/> is not less than <paramref name="slowPeriod"/>.</exception>
    public MacdCalculator(
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (fastPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriod), fastPeriod, "Fast period must be positive.");
        }

        if (slowPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriod), slowPeriod, "Slow period must be positive.");
        }

        if (signalPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriod), signalPeriod, "Signal period must be positive.");
        }

        if (fastPeriod >= slowPeriod)
        {
            throw new ArgumentException($"Fast period ({fastPeriod}) must be less than slow period ({slowPeriod}).", nameof(fastPeriod));
        }

        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
        SignalPeriod = signalPeriod;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the MACD series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing,
    /// so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one signal-line value.
    /// </exception>
    public IReadOnlyList<MacdPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        int minimumBars = SlowPeriod + SignalPeriod;
        if (bars.Count < minimumBars)
        {
            throw new ArgumentException(
                $"At least {minimumBars} price bars are required to compute a " +
                $"{FastPeriod}/{SlowPeriod}/{SignalPeriod} MACD series; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();

        var fastEma = ExponentialMovingAverage(prices, FastPeriod);
        var slowEma = ExponentialMovingAverage(prices, SlowPeriod);

        // The MACD line only exists once the slower EMA is defined.
        int macdOffset = SlowPeriod - 1;
        var macdValues = new List<decimal>(bars.Count - macdOffset);
        for (int i = macdOffset; i < bars.Count; i++)
        {
            macdValues.Add(fastEma[i]!.Value - slowEma[i]!.Value);
        }

        var signalEma = ExponentialMovingAverage(macdValues, SignalPeriod);

        var points = new List<MacdPoint>(macdValues.Count - SignalPeriod + 1);
        decimal? previousHistogram = null;

        for (int j = SignalPeriod - 1; j < macdValues.Count; j++)
        {
            decimal macd = macdValues[j];
            decimal signal = signalEma[j]!.Value;
            decimal histogram = macd - signal;

            var crossover = MacdCrossover.None;
            if (previousHistogram is decimal prev)
            {
                if (prev < 0 && histogram >= 0)
                {
                    crossover = MacdCrossover.Bullish;
                }
                else if (prev > 0 && histogram <= 0)
                {
                    crossover = MacdCrossover.Bearish;
                }
            }
            previousHistogram = histogram;

            int barIndex = macdOffset + j;
            points.Add(new MacdPoint(bars[barIndex].TickerId, bars[barIndex].Date, macd, signal, histogram, crossover));
        }

        return points;
    }

    /// <summary>
    /// Computes an exponential moving average, seeded with a simple moving average of
    /// the first <paramref name="period"/> values. Entries before the seed index are
    /// null because no EMA value is yet defined there. 
    /// </summary>
    private static decimal?[] ExponentialMovingAverage(List<decimal> values, int period)
    {
        var result = new decimal?[values.Count];
        if (values.Count < period)
        {
            return result;
        }

        decimal seedSum = 0;
        for (int i = 0; i < period; i++)
        {
            seedSum += values[i];
        }

        decimal previousEma = seedSum / period;
        result[period - 1] = previousEma;

        decimal smoothing = 2m / (period + 1);
        for (int i = period; i < values.Count; i++)
        {
            decimal currentEma = (values[i] - previousEma) * smoothing + previousEma;
            result[i] = currentEma;
            previousEma = currentEma;
        }

        return result;
    }
}
