using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the (fast) Stochastic Oscillator for a chronological series of
/// <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// %K positions the current close within the high-low range of the trailing
/// <see cref="Period"/> bars. %D is a simple moving average of %K over
/// <see cref="SignalPeriod"/> bars, acting the same role as the MACD signal line.
/// Crossovers of %K over %D are detected the same way <see cref="MacdCalculator"/>
/// detects histogram sign changes.
/// </remarks>
public sealed class StochasticCalculator
{
    public int Period { get; }
    public int SignalPeriod { get; }
    public decimal OverboughtThreshold { get; }
    public decimal OversoldThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback periods and zone thresholds.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> is compared against the
    /// high/low range to compute %K. Defaults to <see cref="StockData.Close"/>; pass
    /// <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> or <paramref name="signalPeriod"/> is not
    /// positive, or when a threshold falls outside the valid 0-100 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="overboughtThreshold"/> is not greater than <paramref name="oversoldThreshold"/>.
    /// </exception>
    public StochasticCalculator(
        int period = 14,
        int signalPeriod = 3,
        decimal overboughtThreshold = 80m,
        decimal oversoldThreshold = 20m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (signalPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriod), signalPeriod, "Signal period must be positive.");
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
        SignalPeriod = signalPeriod;
        OverboughtThreshold = overboughtThreshold;
        OversoldThreshold = oversoldThreshold;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the Stochastic Oscillator series for the given price bars, which must
    /// all belong to the same ticker. Bars are sorted by <see cref="StockData.Date"/>
    /// before computing, so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one %D value.
    /// </exception>
    public IReadOnlyList<StochasticPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        int minimumBars = Period + SignalPeriod - 1;
        if (bars.Count < minimumBars)
        {
            throw new ArgumentException(
                $"At least {minimumBars} price bars are required to compute a " +
                $"{Period}/{SignalPeriod} Stochastic series; {bars.Count} were provided.",
                nameof(series));
        }

        // %K is defined starting at index Period - 1.
        int kCount = bars.Count - Period + 1;
        var percentK = new decimal[kCount];
        for (int i = Period - 1; i < bars.Count; i++)
        {
            decimal highestHigh = decimal.MinValue;
            decimal lowestLow = decimal.MaxValue;
            for (int j = i - Period + 1; j <= i; j++)
            {
                if (bars[j].High > highestHigh)
                {
                    highestHigh = bars[j].High;
                }
                if (bars[j].Low < lowestLow)
                {
                    lowestLow = bars[j].Low;
                }
            }

            decimal range = highestHigh - lowestLow;
            decimal close = _priceSelector(bars[i]);
            // A zero range (every bar in the window identical) has no meaningful position
            // within it; treat it as the midpoint rather than dividing by zero.
            percentK[i - Period + 1] = range == 0m ? 50m : 100m * (close - lowestLow) / range;
        }

        var points = new List<StochasticPoint>(kCount - SignalPeriod + 1);
        decimal? previousDifference = null;

        for (int i = SignalPeriod - 1; i < kCount; i++)
        {
            decimal sum = 0m;
            for (int j = i - SignalPeriod + 1; j <= i; j++)
            {
                sum += percentK[j];
            }
            decimal percentD = sum / SignalPeriod;
            decimal currentK = percentK[i];
            decimal difference = currentK - percentD;

            var crossover = StochasticCrossover.None;
            if (previousDifference is decimal prev)
            {
                if (prev < 0 && difference >= 0)
                {
                    crossover = StochasticCrossover.Bullish;
                }
                else if (prev > 0 && difference <= 0)
                {
                    crossover = StochasticCrossover.Bearish;
                }
            }
            previousDifference = difference;

            var zone = StochasticZone.Neutral;
            if (percentD >= OverboughtThreshold)
            {
                zone = StochasticZone.Overbought;
            }
            else if (percentD <= OversoldThreshold)
            {
                zone = StochasticZone.Oversold;
            }

            int barIndex = i + Period - 1;
            points.Add(new StochasticPoint(bars[barIndex].TickerId, bars[barIndex].Date, currentK, percentD, zone, crossover));
        }

        return points;
    }
}
