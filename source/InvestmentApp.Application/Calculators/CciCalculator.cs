using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Commodity Channel Index (CCI) for a chronological series of
/// <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// CCI compares the typical price (average of high, low, and close) for a bar against a
/// simple moving average of typical price over <see cref="Period"/> bars, scaled by the
/// mean absolute deviation of typical price over the same window. The 0.015 scaling
/// constant is Lambert's original constant, chosen so roughly 70-80% of CCI values fall
/// between -100 and +100 for a typically distributed series.
/// </remarks>
public sealed class CciCalculator
{
    private const decimal ScalingConstant = 0.015m;

    public int Period { get; }
    public decimal OverboughtThreshold { get; }
    public decimal OversoldThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and zone thresholds.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> contributes the "close" leg
    /// of the typical price. Defaults to <see cref="StockData.Close"/>; pass
    /// <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not greater than one.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="overboughtThreshold"/> is not greater than <paramref name="oversoldThreshold"/>.
    /// </exception>
    public CciCalculator(
        int period = 20,
        decimal overboughtThreshold = 100m,
        decimal oversoldThreshold = -100m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than one.");
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
    /// Computes the CCI series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one CCI value.
    /// </exception>
    public IReadOnlyList<CciPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period CCI series; {bars.Count} were provided.",
                nameof(series));
        }

        var typicalPrices = bars.Select(b => (b.High + b.Low + _priceSelector(b)) / 3m).ToList();
        var points = new List<CciPoint>(bars.Count - Period + 1);

        for (int i = Period - 1; i < typicalPrices.Count; i++)
        {
            int windowStart = i - Period + 1;

            decimal sum = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                sum += typicalPrices[j];
            }
            decimal movingAverage = sum / Period;

            decimal sumAbsoluteDeviations = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                sumAbsoluteDeviations += Math.Abs(typicalPrices[j] - movingAverage);
            }
            decimal meanDeviation = sumAbsoluteDeviations / Period;

            // A zero mean deviation (every typical price in the window identical) has no
            // meaningful deviation to scale against; treat CCI as flat rather than divide by zero.
            decimal cci = meanDeviation == 0m
                ? 0m
                : (typicalPrices[i] - movingAverage) / (ScalingConstant * meanDeviation);

            var zone = CciZone.Neutral;
            if (cci >= OverboughtThreshold)
            {
                zone = CciZone.Overbought;
            }
            else if (cci <= OversoldThreshold)
            {
                zone = CciZone.Oversold;
            }

            points.Add(new CciPoint(bars[i].TickerSymbol, bars[i].Date, cci, zone));
        }

        return points;
    }
}
