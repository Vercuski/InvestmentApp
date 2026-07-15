using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes On-Balance Volume (OBV) for a chronological series of <see cref="StockData"/>
/// price bars for a single ticker.
/// </summary>
/// <remarks>
/// OBV is a cumulative running total: volume is added on days the selected price closes
/// higher than the prior bar, subtracted on days it closes lower, and left unchanged on
/// flat days. The first bar has no prior price to compare against, so the series seeds at
/// zero. A simple moving average of OBV acts as a signal line, and OBV crossing above or
/// below that line is treated the same way as the MACD histogram's sign change.
/// </remarks>
public sealed class ObvCalculator
{
    public int SignalPeriod { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given signal-line smoothing period.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> drives the up/down-day
    /// comparison. Defaults to <see cref="StockData.Close"/>; pass <c>s => s.AdjustedClose</c>
    /// instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="signalPeriod"/> is not greater than one.
    /// </exception>
    public ObvCalculator(
        int signalPeriod = 20,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (signalPeriod <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriod), signalPeriod, "Signal period must be greater than one.");
        }

        SignalPeriod = signalPeriod;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the OBV series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one signal-line value.
    /// </exception>
    public IReadOnlyList<ObvPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        if (bars.Count < SignalPeriod)
        {
            throw new ArgumentException(
                $"At least {SignalPeriod} price bars are required to compute an On-Balance " +
                $"Volume series with a {SignalPeriod}-period signal line; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();

        var obv = new decimal[bars.Count];
        obv[0] = 0m;
        for (int i = 1; i < bars.Count; i++)
        {
            if (prices[i] > prices[i - 1])
            {
                obv[i] = obv[i - 1] + bars[i].Volume;
            }
            else if (prices[i] < prices[i - 1])
            {
                obv[i] = obv[i - 1] - bars[i].Volume;
            }
            else
            {
                obv[i] = obv[i - 1];
            }
        }

        var points = new List<ObvPoint>(bars.Count - SignalPeriod + 1);
        decimal? previousDifference = null;

        for (int i = SignalPeriod - 1; i < bars.Count; i++)
        {
            int windowStart = i - SignalPeriod + 1;

            decimal sum = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                sum += obv[j];
            }
            decimal signalLine = sum / SignalPeriod;
            decimal difference = obv[i] - signalLine;

            var trend = ObvTrend.None;
            if (previousDifference is decimal prev)
            {
                if (prev < 0 && difference >= 0)
                {
                    trend = ObvTrend.Bullish;
                }
                else if (prev > 0 && difference <= 0)
                {
                    trend = ObvTrend.Bearish;
                }
            }
            previousDifference = difference;

            points.Add(new ObvPoint(bars[i].TickerSymbol, bars[i].Date, obv[i], signalLine, trend));
        }

        return points;
    }
}
