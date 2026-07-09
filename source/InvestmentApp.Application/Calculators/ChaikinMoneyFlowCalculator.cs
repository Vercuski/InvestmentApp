using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes Chaikin Money Flow (CMF) for a chronological series of <see cref="StockData"/>
/// price bars for a single ticker.
/// </summary>
/// <remarks>
/// Each bar's money flow multiplier positions its close within its own high-low range,
/// from -1 (closed at the low) to +1 (closed at the high), then scales that by volume to
/// get money flow volume. CMF is the sum of money flow volume over <see cref="Period"/>
/// bars divided by the sum of volume over the same window, so it stays roughly within
/// -1 to +1. Positive values indicate buying pressure, negative values selling pressure;
/// <see cref="BullishThreshold"/> and <see cref="BearishThreshold"/> mark how far from
/// zero that pressure needs to be before it's treated as meaningful rather than noise.
/// </remarks>
public sealed class ChaikinMoneyFlowCalculator
{
    public int Period { get; }
    public decimal BullishThreshold { get; }
    public decimal BearishThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and pressure thresholds.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> feeds the close leg of the
    /// money flow multiplier. Defaults to <see cref="StockData.Close"/>; pass
    /// <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not positive, or when a threshold falls
    /// outside the valid -1 to 1 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="bullishThreshold"/> is not greater than <paramref name="bearishThreshold"/>.
    /// </exception>
    public ChaikinMoneyFlowCalculator(
        int period = 20,
        decimal bullishThreshold = 0.05m,
        decimal bearishThreshold = -0.05m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (bullishThreshold > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(bullishThreshold), bullishThreshold, "Bullish threshold cannot exceed 1.");
        }

        if (bearishThreshold < -1m)
        {
            throw new ArgumentOutOfRangeException(nameof(bearishThreshold), bearishThreshold, "Bearish threshold cannot be less than -1.");
        }

        if (bullishThreshold <= bearishThreshold)
        {
            throw new ArgumentException($"Bullish threshold ({bullishThreshold}) must be greater than bearish threshold ({bearishThreshold}).", nameof(bullishThreshold));
        }

        Period = period;
        BullishThreshold = bullishThreshold;
        BearishThreshold = bearishThreshold;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the CMF series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one CMF value.
    /// </exception>
    public IReadOnlyList<ChaikinMoneyFlowPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Chaikin Money Flow series; {bars.Count} were provided.",
                nameof(series));
        }

        var moneyFlowVolumes = new decimal[bars.Count];
        for (int i = 0; i < bars.Count; i++)
        {
            decimal range = bars[i].High - bars[i].Low;
            decimal close = _priceSelector(bars[i]);

            // A zero range (high equals low) leaves the multiplier undefined; treat it as
            // zero pressure rather than divide by zero.
            decimal multiplier = range == 0m
                ? 0m
                : ((close - bars[i].Low) - (bars[i].High - close)) / range;

            moneyFlowVolumes[i] = multiplier * bars[i].Volume;
        }

        var points = new List<ChaikinMoneyFlowPoint>(bars.Count - Period + 1);

        for (int i = Period - 1; i < bars.Count; i++)
        {
            int windowStart = i - Period + 1;

            decimal moneyFlowSum = 0m;
            long volumeSum = 0L;
            for (int j = windowStart; j <= i; j++)
            {
                moneyFlowSum += moneyFlowVolumes[j];
                volumeSum += bars[j].Volume;
            }

            // No volume traded across the whole window leaves CMF undefined; treat it as
            // zero pressure rather than divide by zero.
            decimal cmf = volumeSum == 0L ? 0m : moneyFlowSum / volumeSum;

            var zone = ChaikinMoneyFlowZone.Neutral;
            if (cmf >= BullishThreshold)
            {
                zone = ChaikinMoneyFlowZone.Bullish;
            }
            else if (cmf <= BearishThreshold)
            {
                zone = ChaikinMoneyFlowZone.Bearish;
            }

            points.Add(new ChaikinMoneyFlowPoint(bars[i].TickerId, bars[i].Date, cmf, zone));
        }

        return points;
    }
}
