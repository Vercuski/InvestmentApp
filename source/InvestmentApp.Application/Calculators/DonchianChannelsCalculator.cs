using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Computes Donchian Channels for a chronological series of <see cref="StockData"/> price
/// bars for a single ticker.
/// </summary>
/// <remarks>
/// The upper and lower bands are the highest high and lowest low across the trailing
/// <see cref="Period"/> bars, including the current bar; the middle line is their average.
/// Because the current bar is part of its own window, a bar whose high equals the upper
/// band (or whose low equals the lower band) has, by construction, made a new
/// <see cref="Period"/>-bar extreme &#8212; the classic Donchian/Turtle breakout trigger &#8212;
/// rather than merely sitting at an already-established band the way
/// <see cref="BollingerBandsCalculator"/> or <see cref="KeltnerChannelsCalculator"/> report.
/// </remarks>
public sealed class DonchianChannelsCalculator
{
    public int Period { get; }

    private readonly Func<StockData, decimal> _priceSelector;

    /// <summary>
    /// Creates a calculator with the given lookback period.
    /// </summary>
    /// <param name="priceSelector">
    /// Selects which price field of a <see cref="StockData"/> is reported as the point's
    /// reference price. Defaults to <see cref="StockData.Close"/>; pass
    /// <c>s => s.AdjustedClose</c> instead to account for splits and dividends. Band
    /// breakouts are always evaluated against the bar's actual High/Low, independent of
    /// this selector.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="period"/> is not greater than one.
    /// </exception>
    public DonchianChannelsCalculator(
        int period = 20,
        Func<StockData, decimal>? priceSelector = null)
    {
        if (period <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than one.");
        }

        Period = period;
        _priceSelector = priceSelector ?? (s => s.Close);
    }

    /// <summary>
    /// Computes the Donchian Channels series for the given price bars, which must all
    /// belong to the same ticker. Bars are sorted by <see cref="StockData.Date"/> before
    /// computing, so callers do not need to pre-sort.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="series"/> is null or does not contain enough bars to
    /// produce at least one band value.
    /// </exception>
    public IReadOnlyList<DonchianChannelsPoint> Calculate(IEnumerable<StockData> series)
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
                $"{Period}-period Donchian Channels series; {bars.Count} were provided.",
                nameof(series));
        }

        var points = new List<DonchianChannelsPoint>(bars.Count - Period + 1);

        for (int i = Period - 1; i < bars.Count; i++)
        {
            int windowStart = i - Period + 1;

            decimal highestHigh = decimal.MinValue;
            decimal lowestLow = decimal.MaxValue;
            for (int j = windowStart; j <= i; j++)
            {
                if (bars[j].High > highestHigh)
                {
                    highestHigh = bars[j].High;
                }
                if (bars[j].Low < lowestLow)
                {
                    lowestLow = bars[j].Low;
                }
            }

            decimal middleLine = (highestHigh + lowestLow) / 2m;
            decimal price = _priceSelector(bars[i]);

            var signal = DonchianChannelSignal.WithinChannel;
            if (bars[i].High >= highestHigh)
            {
                signal = DonchianChannelSignal.AboveUpperBand;
            }
            else if (bars[i].Low <= lowestLow)
            {
                signal = DonchianChannelSignal.BelowLowerBand;
            }

            points.Add(new DonchianChannelsPoint(bars[i].TickerSymbol, bars[i].Date, price, highestHigh, middleLine, lowestLow, signal));
        }

        return points;
    }
}
