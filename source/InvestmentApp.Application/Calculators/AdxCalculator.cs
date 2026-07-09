using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Average Directional Index (ADX) for a chronological series of
/// <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// +DM and -DM (directional movement) and True Range are derived from each bar's high/low
/// relative to the prior bar, then Wilder-smoothed the same way <see cref="RsiCalculator"/>
/// smooths average gain/loss. +DI and -DI are the smoothed directional movements expressed
/// as a percentage of smoothed True Range; DX is the normalized absolute difference between
/// them. ADX is itself a Wilder-smoothed average of DX, seeded the same way as the other two
/// stages, which is why it takes roughly twice <see cref="Period"/> bars before the first
/// value is defined. ADX indicates trend strength only, not direction &#8212; pair it with
/// +DI/-DI, or another directional indicator, to determine which way a strong trend is moving.
/// </remarks>
public sealed class AdxCalculator
{
    public int Period { get; }
    public decimal TrendThreshold { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and trend-strength threshold.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> represents the previous
    /// close used in the True Range calculation. Defaults to <see cref="StockData.Close"/>;
    /// pass <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not positive, or when
    /// <paramref name="trendThreshold"/> falls outside the valid 0-100 range.
    /// </exception>
    public AdxCalculator(
        int period = 14,
        decimal trendThreshold = 25m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (trendThreshold <= 0m || trendThreshold > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(trendThreshold), trendThreshold, "Trend threshold must be between 0 and 100.");
        }

        Period = period;
        TrendThreshold = trendThreshold;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the ADX series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one ADX value.
    /// </exception>
    public IReadOnlyList<AdxPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        int minimumBars = 2 * Period;
        if (bars.Count < minimumBars)
        {
            throw new ArgumentException(
                $"At least {minimumBars} price bars are required to compute a " +
                $"{Period}-period ADX series; {bars.Count} were provided.",
                nameof(series));
        }

        // Directional movement and true range: one fewer value than bars, since each
        // needs a comparison against the prior bar.
        int n = bars.Count - 1;
        var plusDm = new decimal[n];
        var minusDm = new decimal[n];
        var trueRanges = new decimal[n];

        for (int i = 1; i < bars.Count; i++)
        {
            decimal upMove = bars[i].High - bars[i - 1].High;
            decimal downMove = bars[i - 1].Low - bars[i].Low;

            plusDm[i - 1] = (upMove > downMove && upMove > 0m) ? upMove : 0m;
            minusDm[i - 1] = (downMove > upMove && downMove > 0m) ? downMove : 0m;

            decimal previousClose = _priceSelector(bars[i - 1]);
            decimal highLow = bars[i].High - bars[i].Low;
            decimal highPrevClose = Math.Abs(bars[i].High - previousClose);
            decimal lowPrevClose = Math.Abs(bars[i].Low - previousClose);
            trueRanges[i - 1] = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
        }

        decimal smoothedPlusDm = Average(plusDm, 0, Period);
        decimal smoothedMinusDm = Average(minusDm, 0, Period);
        decimal smoothedTr = Average(trueRanges, 0, Period);

        // DX (and the +DI/-DI it's built from) become defined as soon as the smoothed
        // DM/TR seed is in place, one bar earlier than ADX itself will be.
        int dxCount = n - Period + 1;
        var dxValues = new decimal[dxCount];
        var plusDiValues = new decimal[dxCount];
        var minusDiValues = new decimal[dxCount];

        plusDiValues[0] = ComputeDirectionalIndex(smoothedPlusDm, smoothedTr);
        minusDiValues[0] = ComputeDirectionalIndex(smoothedMinusDm, smoothedTr);
        dxValues[0] = ComputeDx(plusDiValues[0], minusDiValues[0]);

        for (int k = Period; k < n; k++)
        {
            smoothedPlusDm = (smoothedPlusDm * (Period - 1) + plusDm[k]) / Period;
            smoothedMinusDm = (smoothedMinusDm * (Period - 1) + minusDm[k]) / Period;
            smoothedTr = (smoothedTr * (Period - 1) + trueRanges[k]) / Period;

            int idx = k - Period + 1;
            plusDiValues[idx] = ComputeDirectionalIndex(smoothedPlusDm, smoothedTr);
            minusDiValues[idx] = ComputeDirectionalIndex(smoothedMinusDm, smoothedTr);
            dxValues[idx] = ComputeDx(plusDiValues[idx], minusDiValues[idx]);
        }

        // ADX seeds the same way as the DM/TR smoothing did: a simple average of the
        // first Period DX values, then Wilder-smoothed from there.
        decimal adx = Average(dxValues, 0, Period);
        var points = new List<AdxPoint>(dxCount - Period + 1);

        int firstBarIndex = 2 * Period - 1;
        points.Add(BuildPoint(bars[firstBarIndex], adx, plusDiValues[Period - 1], minusDiValues[Period - 1]));

        for (int m = Period; m < dxCount; m++)
        {
            adx = (adx * (Period - 1) + dxValues[m]) / Period;
            int barIndex = Period + m;
            points.Add(BuildPoint(bars[barIndex], adx, plusDiValues[m], minusDiValues[m]));
        }

        return points;
    }

    private AdxPoint BuildPoint(StockData bar, decimal adx, decimal plusDi, decimal minusDi)
    {
        var strength = adx >= TrendThreshold ? AdxTrendStrength.Strong : AdxTrendStrength.Weak;
        return new AdxPoint(bar.TickerId, bar.Date, adx, plusDi, minusDi, strength);
    }

    private static decimal Average(IReadOnlyList<decimal> values, int start, int count)
    {
        decimal sum = 0m;
        for (int i = start; i < start + count; i++)
        {
            sum += values[i];
        }
        return sum / count;
    }

    private static decimal ComputeDirectionalIndex(decimal smoothedDirectionalMovement, decimal smoothedTrueRange)
    {
        return smoothedTrueRange == 0m ? 0m : 100m * smoothedDirectionalMovement / smoothedTrueRange;
    }

    private static decimal ComputeDx(decimal plusDi, decimal minusDi)
    {
        decimal sum = plusDi + minusDi;
        return sum == 0m ? 0m : 100m * Math.Abs(plusDi - minusDi) / sum;
    }
}
