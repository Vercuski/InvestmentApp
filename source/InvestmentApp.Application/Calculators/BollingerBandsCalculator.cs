using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes Bollinger Bands for a chronological series of <see cref="Stock"/> price
/// bars for a single ticker.
/// </summary>
/// <remarks>
/// The middle band is a simple moving average over <see cref="Period"/> bars. The
/// upper and lower bands sit <see cref="StandardDeviations"/> population standard
/// deviations above and below the middle band. Widening bands indicate rising
/// volatility; narrowing bands ("the squeeze") often precede a sharp directional move.
/// </remarks>
public sealed class BollingerBandsCalculator
{
    public int Period { get; }
    public decimal StandardDeviations { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period and band width.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="Stock"/> feeds the calculation.
    /// Defaults to <see cref="Stock.Close"/>; pass <c>s => s.AdjustedClose</c> instead
    /// to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not greater than one, or when
    /// <paramref name="standardDeviations"/> is not positive.
    /// </exception>
    public BollingerBandsCalculator(
        int period = 20,
        decimal standardDeviations = 2m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than one.");
        }

        if (standardDeviations <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(standardDeviations), standardDeviations, "Standard deviation multiplier must be positive.");
        }

        Period = period;
        StandardDeviations = standardDeviations;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the Bollinger Bands series for the given price bars, which must all
    /// belong to the same ticker. Bars are sorted by <see cref="Stock.PriceDate"/>
    /// before computing, so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one band value.
    /// </exception>
    public IReadOnlyList<BollingerBandsPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Bollinger Bands series; {bars.Count} were provided.",
                nameof(series));
        }

        var prices = bars.Select(_priceSelector).ToList();
        var points = new List<BollingerBandsPoint>(prices.Count - Period + 1);

        for (int i = Period - 1; i < prices.Count; i++)
        {
            int windowStart = i - Period + 1;

            decimal sum = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                sum += prices[j];
            }
            decimal middleBand = sum / Period;

            decimal sumSquaredDeviations = 0m;
            for (int j = windowStart; j <= i; j++)
            {
                decimal deviation = prices[j] - middleBand;
                sumSquaredDeviations += deviation * deviation;
            }
            decimal variance = sumSquaredDeviations / Period;

            // decimal has no Sqrt; the precision loss from a double round-trip here
            // is negligible for a volatility band width and standard for this indicator.
            decimal standardDeviation = (decimal)Math.Sqrt((double)variance);

            decimal upperBand = middleBand + StandardDeviations * standardDeviation;
            decimal lowerBand = middleBand - StandardDeviations * standardDeviation;
            decimal price = prices[i];

            var signal = BollingerBandSignal.WithinBands;
            if (price > upperBand)
            {
                signal = BollingerBandSignal.AboveUpperBand;
            }
            else if (price < lowerBand)
            {
                signal = BollingerBandSignal.BelowLowerBand;
            }

            points.Add(new BollingerBandsPoint(bars[i].TickerId, bars[i].Date, price, middleBand, upperBand, lowerBand, signal));
        }

        return points;
    }
}
