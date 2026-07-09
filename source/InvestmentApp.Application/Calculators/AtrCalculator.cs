using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Average True Range (ATR) for a chronological series of <see cref="StockData"/>
/// price bars for a single ticker.
/// </summary>
/// <remarks>
/// True Range for a bar is the greatest of: the current high-low range, the distance from
/// the current high to the previous close, and the distance from the current low to the
/// previous close. ATR is a Wilder-smoothed average of True Range, seeded with a simple
/// average of the first <see cref="Period"/> values, the same seeding approach used by
/// <see cref="RsiCalculator"/>. ATR measures volatility magnitude only; it does not
/// indicate direction and is typically used for position sizing or stop-loss placement
/// alongside a directional indicator.
/// </remarks>
public sealed class AtrCalculator
{
    public int Period { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> represents the previous
    /// close used in the True Range calculation. Defaults to <see cref="StockData.Close"/>;
    /// pass <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is not positive.</exception>
    public AtrCalculator(
        int period = 14,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be positive.");
        }

        Period = period;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the ATR series for the given price bars, which must all belong to the
    /// same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing, so
    /// callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one ATR value.
    /// </exception>
    public IReadOnlyList<AtrPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period ATR series; {bars.Count} were provided.",
                nameof(series));
        }

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

        decimal seedSum = 0m;
        for (int i = 0; i < Period; i++)
        {
            seedSum += trueRanges[i];
        }
        decimal averageTrueRange = seedSum / Period;

        var points = new List<AtrPoint>(bars.Count - Period + 1)
        {
            new AtrPoint(bars[Period - 1].TickerId, bars[Period - 1].Date, averageTrueRange)
        };

        for (int i = Period; i < bars.Count; i++)
        {
            // Wilder's smoothing: weight the running average by (Period - 1) parts
            // history to 1 part the newest true range value.
            averageTrueRange = (averageTrueRange * (Period - 1) + trueRanges[i]) / Period;
            points.Add(new AtrPoint(bars[i].TickerId, bars[i].Date, averageTrueRange));
        }

        return points;
    }
}
