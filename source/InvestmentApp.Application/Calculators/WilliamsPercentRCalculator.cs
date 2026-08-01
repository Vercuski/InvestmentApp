using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes Williams %R for a chronological series of <see cref="StockData"/> price bars
/// for a single ticker.
/// </summary>
/// <remarks>
/// %R positions the current close within the high-low range of the trailing
/// <see cref="Period"/> bars, the same window <see cref="StochasticCalculator"/> uses for
/// %K, but expressed as a negative percentage: 0 means the close sits at the period high,
/// -100 means it sits at the period low. Because the scale runs 0 to -100 rather than 0 to
/// 100, its overbought/oversold thresholds are the negative mirror of a typical
/// Stochastic's, and it is reported as a single raw line with no signal-line smoothing.
/// </remarks>
public sealed class WilliamsPercentRCalculator
{
    public int Period { get; }
    public decimal OverboughtThreshold { get; }
    public decimal OversoldThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and zone thresholds.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> is compared against the
    /// high/low range to compute %R. Defaults to <see cref="StockData.Close"/>; pass
    /// <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not greater than one, or when a threshold
    /// falls outside the valid -100 to 0 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="overboughtThreshold"/> is not greater than <paramref name="oversoldThreshold"/>.
    /// </exception>
    public WilliamsPercentRCalculator(
        int period = 14,
        decimal overboughtThreshold = -20m,
        decimal oversoldThreshold = -80m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than one.");
        }

        if (overboughtThreshold > 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(overboughtThreshold), overboughtThreshold, "Overbought threshold cannot exceed 0.");
        }

        if (oversoldThreshold < -100m)
        {
            throw new ArgumentOutOfRangeException(nameof(oversoldThreshold), oversoldThreshold, "Oversold threshold cannot be less than -100.");
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
    /// Computes the Williams %R series for the given price bars, which must all belong to
    /// the same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing,
    /// so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one %R value.
    /// </exception>
    public IReadOnlyList<WilliamsPercentRPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        if (bars.Count < Period)
        {
            throw new ArgumentException(
                $"At least {Period} price bars are required to compute a " +
                $"{Period}-period Williams %R series; {bars.Count} were provided.",
                nameof(series));
        }

        var points = new List<WilliamsPercentRPoint>(bars.Count - Period + 1);

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
            decimal percentR = range == 0m ? -50m : -100m * (highestHigh - close) / range;

            var zone = WilliamsPercentRZone.Neutral;
            if (percentR >= OverboughtThreshold)
            {
                zone = WilliamsPercentRZone.Overbought;
            }
            else if (percentR <= OversoldThreshold)
            {
                zone = WilliamsPercentRZone.Oversold;
            }

            points.Add(new WilliamsPercentRPoint(bars[i].TickerSymbol, bars[i].Date, percentR, zone));
        }

        return points;
    }
}
