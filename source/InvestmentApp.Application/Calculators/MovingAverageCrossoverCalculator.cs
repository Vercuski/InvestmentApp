using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes a simple-moving-average crossover for a chronological series of
/// <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// This is the classic "Golden Cross"/"Death Cross" definition: two simple moving
/// averages (traditionally 50-period and 200-period) compared directly against each
/// other, with no additional smoothing stage. A crossover is detected the same way
/// <see cref="MacdCalculator"/> detects histogram sign changes, applied here to the
/// difference between the fast and slow averages instead of the MACD histogram.
/// </remarks>
public sealed class MovingAverageCrossoverCalculator
{
    public int FastPeriod { get; }
    public int SlowPeriod { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given fast and slow lookback periods.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> feeds both moving averages.
    /// Defaults to <see cref="StockData.Close"/>; pass <c>s => s.AdjustedClose</c> instead
    /// to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fastPeriod"/> is not less than <paramref name="slowPeriod"/>.</exception>
    public MovingAverageCrossoverCalculator(
        int fastPeriod = 50,
        int slowPeriod = 200,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (fastPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriod), fastPeriod, "Fast period must be positive.");
        }

        if (slowPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriod), slowPeriod, "Slow period must be positive.");
        }

        if (fastPeriod >= slowPeriod)
        {
            throw new ArgumentException($"Fast period ({fastPeriod}) must be less than slow period ({slowPeriod}).", nameof(fastPeriod));
        }

        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the crossover series for the given price bars, which must all belong to
    /// the same ticker. Bars are sorted by <see cref="StockData.Date"/> before computing,
    /// so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one crossover value.
    /// </exception>
    public IReadOnlyList<MovingAverageCrossoverPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        if (bars.Count < SlowPeriod)
        {
            throw new ArgumentException(
                $"At least {SlowPeriod} price bars are required to compute a " +
                $"{FastPeriod}/{SlowPeriod} moving average crossover series; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();
        var points = new List<MovingAverageCrossoverPoint>(bars.Count - SlowPeriod + 1);
        decimal? previousDifference = null;

        for (int i = SlowPeriod - 1; i < prices.Count; i++)
        {
            decimal fastAverage = SimpleMovingAverage(prices, i, FastPeriod);
            decimal slowAverage = SimpleMovingAverage(prices, i, SlowPeriod);
            decimal difference = fastAverage - slowAverage;

            var crossover = MovingAverageCrossover.None;
            if (previousDifference is decimal prev)
            {
                if (prev < 0 && difference >= 0)
                {
                    crossover = MovingAverageCrossover.Bullish;
                }
                else if (prev > 0 && difference <= 0)
                {
                    crossover = MovingAverageCrossover.Bearish;
                }
            }
            previousDifference = difference;

            points.Add(new MovingAverageCrossoverPoint(bars[i].TickerSymbol, bars[i].Date, fastAverage, slowAverage, difference, crossover));
        }

        return points;
    }

    private static decimal SimpleMovingAverage(List<decimal> prices, int endIndex, int period)
    {
        decimal sum = 0m;
        for (int j = endIndex - period + 1; j <= endIndex; j++)
        {
            sum += prices[j];
        }
        return sum / period;
    }
}
