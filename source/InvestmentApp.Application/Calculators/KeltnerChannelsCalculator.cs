using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes Keltner Channels for a chronological series of <see cref="StockData"/> price
/// bars for a single ticker.
/// </summary>
/// <remarks>
/// The middle line is an exponential moving average of the selected price over
/// <see cref="Period"/> bars, seeded with a simple average of the first
/// <see cref="Period"/> values the same way <see cref="MacdCalculator"/> seeds its EMAs.
/// The upper and lower bands sit <see cref="AtrMultiplier"/> Average True Range values
/// above and below the middle line, using the same Wilder-smoothed True Range
/// <see cref="AtrCalculator"/> computes, calculated independently here rather than
/// depending on that calculator directly. Where Bollinger Bands widen and narrow with
/// price standard deviation, Keltner Channels widen and narrow with True Range, which
/// makes them react less to single-bar price spikes.
/// </remarks>
public sealed class KeltnerChannelsCalculator
{
    public int Period { get; }
    public decimal AtrMultiplier { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and band width.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> feeds the middle-line EMA
    /// and the previous-close leg of the True Range calculation. Defaults to
    /// <see cref="StockData.Close"/>; pass <c>s => s.AdjustedClose</c> instead to account
    /// for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not greater than one, or when
    /// <paramref name="atrMultiplier"/> is not positive.
    /// </exception>
    public KeltnerChannelsCalculator(
        int period = 20,
        decimal atrMultiplier = 2m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than one.");
        }

        if (atrMultiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(atrMultiplier), atrMultiplier, "ATR multiplier must be positive.");
        }

        Period = period;
        AtrMultiplier = atrMultiplier;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the Keltner Channels series for the given price bars, which must all
    /// belong to the same ticker. Bars are sorted by <see cref="StockData.Date"/> before
    /// computing, so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one band value.
    /// </exception>
    public IReadOnlyList<KeltnerChannelsPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Keltner Channels series; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();
        var middleLine = ExponentialMovingAverage(prices, Period);
        var atr = AverageTrueRange(bars, Period);

        var points = new List<KeltnerChannelsPoint>(bars.Count - Period + 1);

        for (int i = Period - 1; i < bars.Count; i++)
        {
            decimal middle = middleLine[i]!.Value;
            decimal band = AtrMultiplier * atr[i]!.Value;
            decimal upperBand = middle + band;
            decimal lowerBand = middle - band;
            decimal price = prices[i];

            var signal = KeltnerChannelSignal.WithinChannel;
            if (price > upperBand)
            {
                signal = KeltnerChannelSignal.AboveUpperBand;
            }
            else if (price < lowerBand)
            {
                signal = KeltnerChannelSignal.BelowLowerBand;
            }

            points.Add(new KeltnerChannelsPoint(bars[i].TickerSymbol, bars[i].Date, price, middle, upperBand, lowerBand, signal));
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

        decimal seedSum = 0m;
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

    /// <summary>
    /// Computes a Wilder-smoothed Average True Range, seeded with a simple average of
    /// the first <paramref name="period"/> true range values, the same technique
    /// <see cref="AtrCalculator"/> uses.
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
