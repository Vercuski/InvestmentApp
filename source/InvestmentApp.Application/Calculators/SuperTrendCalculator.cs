using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the SuperTrend indicator for a chronological series of <see cref="StockData"/>
/// price bars for a single ticker.
/// </summary>
/// <remarks>
/// Each bar's basic upper/lower bands sit <see cref="Multiplier"/> Average True Range
/// values above and below the bar's high-low midpoint, using the same Wilder-smoothed
/// True Range <see cref="AtrCalculator"/> computes, calculated independently here rather
/// than depending on that calculator directly (the same choice <see cref="KeltnerChannelsCalculator"/>
/// makes). The final bands only move in the direction that tightens the trail: the final
/// upper band can only fall (or reset if price closes above it), and the final lower band
/// can only rise (or reset if price closes below it). The SuperTrend line itself is
/// whichever final band is currently active; price closing through that band flips the
/// active band and therefore the trend.
/// </remarks>
public sealed class SuperTrendCalculator
{
    public int Period { get; }
    public decimal Multiplier { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given ATR lookback period and band multiplier.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> represents the previous
    /// close used in the True Range calculation and the close compared against the bands.
    /// Defaults to <see cref="StockData.Close"/>; pass <c>s => s.AdjustedClose</c> instead
    /// to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not positive, or when
    /// <paramref name="multiplier"/> is not positive.
    /// </exception>
    public SuperTrendCalculator(
        int period = 10,
        decimal multiplier = 3m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        if (multiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Multiplier must be positive.");
        }

        Period = period;
        Multiplier = multiplier;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the SuperTrend series for the given price bars, which must all belong to
    /// the same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing,
    /// so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one SuperTrend value.
    /// </exception>
    public IReadOnlyList<SuperTrendPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period SuperTrend series; {bars.Count} were provided.",
                nameof(series));
        }

        var atr = AverageTrueRange(bars, Period);
        var points = new List<SuperTrendPoint>(bars.Count - Period + 1);

        int seedIndex = Period - 1;
        decimal seedMidpoint = (bars[seedIndex].High + bars[seedIndex].Low) / 2m;
        decimal seedBand = Multiplier * atr[seedIndex]!.Value;
        decimal finalUpperBand = seedMidpoint + seedBand;
        decimal finalLowerBand = seedMidpoint - seedBand;

        decimal seedClose = _priceSelector(bars[seedIndex]);
        var trend = seedClose <= finalUpperBand ? SuperTrendTrend.Bearish : SuperTrendTrend.Bullish;
        decimal value = trend == SuperTrendTrend.Bearish ? finalUpperBand : finalLowerBand;

        points.Add(new SuperTrendPoint(bars[seedIndex].TickerSymbol, bars[seedIndex].Date, value, trend));

        for (int i = Period; i < bars.Count; i++)
        {
            decimal midpoint = (bars[i].High + bars[i].Low) / 2m;
            decimal band = Multiplier * atr[i]!.Value;
            decimal basicUpperBand = midpoint + band;
            decimal basicLowerBand = midpoint - band;

            decimal previousClose = _priceSelector(bars[i - 1]);
            finalUpperBand = (basicUpperBand < finalUpperBand || previousClose > finalUpperBand)
                ? basicUpperBand
                : finalUpperBand;
            finalLowerBand = (basicLowerBand > finalLowerBand || previousClose < finalLowerBand)
                ? basicLowerBand
                : finalLowerBand;

            decimal close = _priceSelector(bars[i]);
            if (trend == SuperTrendTrend.Bearish)
            {
                trend = close > finalUpperBand ? SuperTrendTrend.Bullish : SuperTrendTrend.Bearish;
            }
            else
            {
                trend = close < finalLowerBand ? SuperTrendTrend.Bearish : SuperTrendTrend.Bullish;
            }

            value = trend == SuperTrendTrend.Bearish ? finalUpperBand : finalLowerBand;
            points.Add(new SuperTrendPoint(bars[i].TickerSymbol, bars[i].Date, value, trend));
        }

        return points;
    }

    /// <summary>
    /// Computes a Wilder-smoothed Average True Range, seeded with a simple average of the
    /// first <paramref name="period"/> true range values, the same technique
    /// <see cref="AtrCalculator"/> and <see cref="KeltnerChannelsCalculator"/> use.
    /// </summary>
    private decimal?[] AverageTrueRange(List<StockData> bars, int period)
    {
        var trueRanges = new decimal[bars.Count];
        trueRanges[0] = bars[0].High - bars[0].Low;
        for (int i = 1; i < bars.Count; i++)
        {
            decimal previousClose = _priceSelector(bars[i - 1]);
            decimal highLow = bars[i].High - bars[i].Low;
            decimal highPrevClose = Math.Abs(bars[i].High - previousClose);
            decimal lowPrevClose = Math.Abs(bars[i].Low - previousClose);
            trueRanges[i] = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
        }

        var result = new decimal?[bars.Count];

        decimal seedSum = 0m;
        for (int i = 0; i < period; i++)
        {
            seedSum += trueRanges[i];
        }

        decimal previousAtr = seedSum / period;
        result[period - 1] = previousAtr;

        for (int i = period; i < bars.Count; i++)
        {
            previousAtr = (previousAtr * (period - 1) + trueRanges[i]) / period;
            result[i] = previousAtr;
        }

        return result;
    }
}
