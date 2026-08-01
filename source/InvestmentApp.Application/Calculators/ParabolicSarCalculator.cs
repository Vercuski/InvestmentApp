using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes the Parabolic SAR (Stop And Reverse) indicator for a chronological series of
/// <see cref="StockData"/> price bars for a single ticker.
/// </summary>
/// <remarks>
/// The SAR trails price from below during an uptrend and from above during a downtrend,
/// stepping toward the trend's extreme point (the highest high seen in an uptrend, or the
/// lowest low in a downtrend) by an acceleration factor that grows by
/// <see cref="AccelerationFactorStep"/> each time a new extreme point is made, capped at
/// <see cref="AccelerationFactorMax"/>. When price crosses the SAR, the trend reverses: the
/// SAR resets to the extreme point of the trend that just ended, the extreme point resets
/// to the current bar's high/low, and the acceleration factor resets to
/// <see cref="AccelerationFactorStart"/>. Unlike <see cref="SuperTrendCalculator"/>, which
/// derives its bands from Average True Range, Parabolic SAR accelerates independently of
/// volatility, which is why it is typically paired with a stop-loss cap rather than used
/// as one directly.
/// </remarks>
public sealed class ParabolicSarCalculator
{
    public decimal AccelerationFactorStart { get; }
    public decimal AccelerationFactorStep { get; }
    public decimal AccelerationFactorMax { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given acceleration factor schedule.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> decides the initial trend
    /// direction between the first two bars. Defaults to <see cref="StockData.Close"/>;
    /// pass <c>s => s.AdjustedClose</c> instead to account for splits and dividends.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="accelerationFactorStart"/> or
    /// <paramref name="accelerationFactorStep"/> is not positive.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="accelerationFactorMax"/> is less than
    /// <paramref name="accelerationFactorStart"/>.
    /// </exception>
    public ParabolicSarCalculator(
        decimal accelerationFactorStart = 0.02m,
        decimal accelerationFactorStep = 0.02m,
        decimal accelerationFactorMax = 0.2m,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (accelerationFactorStart <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(accelerationFactorStart), accelerationFactorStart, "Acceleration factor start must be positive.");
        }

        if (accelerationFactorStep <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(accelerationFactorStep), accelerationFactorStep, "Acceleration factor step must be positive.");
        }

        if (accelerationFactorMax < accelerationFactorStart)
        {
            throw new ArgumentException($"Acceleration factor max ({accelerationFactorMax}) must be greater than or equal to the start value ({accelerationFactorStart}).", nameof(accelerationFactorMax));
        }

        AccelerationFactorStart = accelerationFactorStart;
        AccelerationFactorStep = accelerationFactorStep;
        AccelerationFactorMax = accelerationFactorMax;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the Parabolic SAR series for the given price bars, which must all belong
    /// to the same ticker. Bars are sorted by <see cref="StockData.Date"/> before
    /// computing, so callers do not need to pre-sort. The first bar is consumed only to
    /// seed the initial trend and extreme point, so the first output point corresponds to
    /// the second bar.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain at least two bars.
    /// </exception>
    public IReadOnlyList<ParabolicSarPoint> Calculate(IEnumerable<StockData> series)
    {
        if (series is null)
        {
            throw new ArgumentException("Series cannot be null.", nameof(series));
        }

        var bars = series.OrderBy(s => s.Date).ToList();

        if (bars.Count < 2)
        {
            throw new ArgumentException(
                $"At least 2 price bars are required to compute a Parabolic SAR series; {bars.Count} were provided.",
                nameof(series));
        }

        bool isUptrend = _priceSelector(bars[1]) >= _priceSelector(bars[0]);
        decimal sar = isUptrend ? bars[0].Low : bars[0].High;
        decimal extremePoint = isUptrend ? bars[0].High : bars[0].Low;
        decimal af = AccelerationFactorStart;

        var points = new List<ParabolicSarPoint>(bars.Count - 1);

        for (int i = 1; i < bars.Count; i++)
        {
            decimal candidateSar = sar + af * (extremePoint - sar);

            decimal priorLow = bars[i - 1].Low;
            decimal priorHigh = bars[i - 1].High;
            if (isUptrend)
            {
                decimal clampLow = i >= 2 ? Math.Min(priorLow, bars[i - 2].Low) : priorLow;
                candidateSar = Math.Min(candidateSar, clampLow);
            }
            else
            {
                decimal clampHigh = i >= 2 ? Math.Max(priorHigh, bars[i - 2].High) : priorHigh;
                candidateSar = Math.Max(candidateSar, clampHigh);
            }

            bool reversed = isUptrend ? bars[i].Low < candidateSar : bars[i].High > candidateSar;
            if (reversed)
            {
                sar = extremePoint;
                isUptrend = !isUptrend;
                extremePoint = isUptrend ? bars[i].High : bars[i].Low;
                af = AccelerationFactorStart;
            }
            else
            {
                sar = candidateSar;
                if (isUptrend && bars[i].High > extremePoint)
                {
                    extremePoint = bars[i].High;
                    af = Math.Min(af + AccelerationFactorStep, AccelerationFactorMax);
                }
                else if (!isUptrend && bars[i].Low < extremePoint)
                {
                    extremePoint = bars[i].Low;
                    af = Math.Min(af + AccelerationFactorStep, AccelerationFactorMax);
                }
            }

            var trend = isUptrend ? ParabolicSarTrend.Bullish : ParabolicSarTrend.Bearish;
            points.Add(new ParabolicSarPoint(bars[i].TickerSymbol, bars[i].Date, sar, trend));
        }

        return points;
    }
}
