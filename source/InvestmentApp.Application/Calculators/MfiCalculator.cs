using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Money Flow Index (MFI) for a chronological series of <see cref="StockData"/>
/// price bars for a single ticker.
/// </summary>
/// <remarks>
/// MFI is often described as a volume-weighted RSI: each bar's typical price (average of
/// high, low, and close, the same figure <see cref="CciCalculator"/> uses) is multiplied by
/// volume to get raw money flow, which is classed as positive or negative based on whether
/// typical price rose or fell from the prior bar &#8212; the same up/down-day comparison
/// <see cref="ObvCalculator"/> makes, but weighted by magnitude and volume rather than a
/// flat running total. The money flow ratio (summed positive over summed negative money
/// flow across <see cref="Period"/> bars) is normalized into the same 0-100 range as RSI.
/// </remarks>
public sealed class MfiCalculator
{
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
    /// Thrown when <paramref name="period"/> is not positive, or when a threshold falls
    /// outside the valid 0-100 range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="overboughtThreshold"/> is not greater than <paramref name="oversoldThreshold"/>.
    /// </exception>
    public MfiCalculator(
        int period = 14,
        decimal overboughtThreshold = 80m,
        decimal oversoldThreshold = 20m,
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
    /// Computes the MFI series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one MFI value.
    /// </exception>
    public IReadOnlyList<MfiPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Money Flow Index series; {bars.Count} were provided.",
                nameof(series));
        }

        var typicalPrices = bars.Select(b => (b.High + b.Low + _priceSelector(b)) / 3m).ToList();

        // Raw money flow is defined starting at index 1, since each bar needs a comparison
        // against the prior bar's typical price to know whether it counts as positive or
        // negative flow.
        int n = bars.Count - 1;
        var positiveFlow = new decimal[n];
        var negativeFlow = new decimal[n];

        for (int i = 1; i < bars.Count; i++)
        {
            decimal rawMoneyFlow = typicalPrices[i] * bars[i].Volume;
            if (typicalPrices[i] > typicalPrices[i - 1])
            {
                positiveFlow[i - 1] = rawMoneyFlow;
            }
            else if (typicalPrices[i] < typicalPrices[i - 1])
            {
                negativeFlow[i - 1] = rawMoneyFlow;
            }
        }

        var points = new List<MfiPoint>(n - Period + 1);

        for (int i = Period - 1; i < n; i++)
        {
            int windowStart = i - Period + 1;

            decimal positiveSum = 0m;
            decimal negativeSum = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                positiveSum += positiveFlow[j];
                negativeSum += negativeFlow[j];
            }

            // No negative flow at all in the window is the strongest possible reading;
            // no positive flow at all is the weakest. Both would otherwise divide by zero.
            decimal mfi;
            if (negativeSum == 0m)
            {
                mfi = 100m;
            }
            else if (positiveSum == 0m)
            {
                mfi = 0m;
            }
            else
            {
                decimal moneyRatio = positiveSum / negativeSum;
                mfi = 100m - 100m / (1m + moneyRatio);
            }

            var zone = MfiZone.Neutral;
            if (mfi >= OverboughtThreshold)
            {
                zone = MfiZone.Overbought;
            }
            else if (mfi <= OversoldThreshold)
            {
                zone = MfiZone.Oversold;
            }

            int barIndex = i + 1;
            points.Add(new MfiPoint(bars[barIndex].TickerSymbol, bars[barIndex].Date, mfi, zone));
        }

        return points;
    }
}
