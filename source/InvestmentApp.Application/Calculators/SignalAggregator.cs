using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Application.Calculators;

/// <summary>
/// Composes the already-computed outputs of individual indicator calculators into a
/// single <see cref="TradeSignalPoint"/> series using a regime-switching strategy.
/// </summary>
/// <remarks>
/// <para>
/// ADX's <see cref="AdxPoint.TrendStrength"/> selects which sub-strategy applies at each
/// point, so the trend-strength threshold only needs to be configured once, on the
/// <see cref="AdxCalculator"/> itself, and is honored automatically here:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <b>Trending</b> (<see cref="AdxTrendStrength.Strong"/>): follow <see cref="MacdPoint.Crossover"/>.
/// A bullish/bearish MACD crossover becomes a Buy/Sell. Confidence is layered from two
/// independent confirmations: <see cref="ObvPoint.Trend"/> agreeing (volume behind the move)
/// and price breaking the same-direction <see cref="KeltnerChannelsPoint"/> band (genuine
/// volatility expansion, not just a weak EMA cross). Both agreeing is the strongest read;
/// neither agreeing is the weakest signal that still clears the MACD-cross bar.
/// </description></item>
/// <item><description>
/// <b>Ranging</b> (<see cref="AdxTrendStrength.Weak"/>): trade mean-reversion. Price at the
/// lower Bollinger Band together with an oversold RSI is a Buy; price at the upper band
/// together with an overbought RSI is a Sell. MACD crossovers are ignored in this regime,
/// since trend-following signals whipsaw in a ranging market. <see cref="CciPoint.Zone"/>
/// agreeing with RSI's zone raises confidence, since it's a second oscillator built on
/// different math confirming the same overbought/oversold read. <see cref="ChaikinMoneyFlowPoint.Zone"/>
/// acts as a veto rather than a vote: money flow still running against the reversal (heavy
/// distribution under a Buy setup, heavy accumulation under a Sell setup) is a "falling
/// knife" warning that suppresses the signal entirely, since it's independent information
/// a price oscillator can't see.
/// </description></item>
/// </list>
/// <para>
/// Unlike a single-indicator calculator, this class does not compute anything from raw
/// <see cref="StockData"/> itself &#8212; it only aligns and composes indicator series that
/// have already been calculated, plus the raw closing price (for stop-loss pricing).
/// </para>
/// </remarks>
public sealed class SignalAggregator
{
    // Confidence tiers. Kept as named constants rather than inline literals now that both
    // regimes produce more than two tiers, so the meaning of each number stays legible at
    // the call site in Evaluate().
    private const decimal FullConfidence = 1.0m;
    private const decimal HighConfidence = 0.85m;
    private const decimal ModerateConfidence = 0.75m;
    private const decimal LowConfidence = 0.6m;
    private const decimal NoConfidence = 0m;

    /// <summary>The multiple of ATR subtracted from (Buy) or added to (Sell) the closing price to derive a stop-loss.</summary>
    public decimal AtrStopLossMultiplier { get; }

    /// <summary>
    /// Creates an aggregator with the given ATR stop-loss multiplier.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="atrStopLossMultiplier"/> is not positive.</exception>
    public SignalAggregator(decimal atrStopLossMultiplier = 2m)
    {
        if (atrStopLossMultiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(atrStopLossMultiplier), atrStopLossMultiplier, "ATR stop-loss multiplier must be positive.");
        }

        AtrStopLossMultiplier = atrStopLossMultiplier;
    }

    /// <summary>
    /// Composes a <see cref="TradeSignalPoint"/> series from the given price bars and
    /// pre-computed indicator series, all of which must belong to the same ticker.
    /// Points are only produced for dates present in every one of the required series
    /// (an inner join), since MACD, RSI, ADX, etc. each become defined after a different
    /// number of warm-up bars.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="priceSeries"/> or any indicator series is null or empty,
    /// or when none of the series share a common <c>PriceDate</c>.
    /// </exception>
    public IReadOnlyList<TradeSignalPoint> Calculate(
        IEnumerable<StockData> priceSeries,
        IReadOnlyList<MacdPoint> macdSeries,
        IReadOnlyList<RsiPoint> rsiSeries,
        IReadOnlyList<BollingerBandsPoint> bollingerBandsSeries,
        IReadOnlyList<AdxPoint> adxSeries,
        IReadOnlyList<ObvPoint> obvSeries,
        IReadOnlyList<AtrPoint> atrSeries,
        IReadOnlyList<CciPoint> cciSeries,
        IReadOnlyList<ChaikinMoneyFlowPoint> chaikinMoneyFlowSeries,
        IReadOnlyList<KeltnerChannelsPoint> keltnerChannelsSeries)
    {
        var bars = RequireNonEmpty(priceSeries?.OrderBy(s => s.Date).ToList(), nameof(priceSeries));
        RequireNonEmpty(macdSeries, nameof(macdSeries));
        RequireNonEmpty(rsiSeries, nameof(rsiSeries));
        RequireNonEmpty(bollingerBandsSeries, nameof(bollingerBandsSeries));
        RequireNonEmpty(adxSeries, nameof(adxSeries));
        RequireNonEmpty(obvSeries, nameof(obvSeries));
        RequireNonEmpty(atrSeries, nameof(atrSeries));
        RequireNonEmpty(cciSeries, nameof(cciSeries));
        RequireNonEmpty(chaikinMoneyFlowSeries, nameof(chaikinMoneyFlowSeries));
        RequireNonEmpty(keltnerChannelsSeries, nameof(keltnerChannelsSeries));

        var pricesByDate = bars.ToDictionary(s => s.Date, s => s.Close);
        var macdByDate = macdSeries.ToDictionary(p => p.PriceDate);
        var rsiByDate = rsiSeries.ToDictionary(p => p.PriceDate);
        var bollingerByDate = bollingerBandsSeries.ToDictionary(p => p.PriceDate);
        var adxByDate = adxSeries.ToDictionary(p => p.PriceDate);
        var obvByDate = obvSeries.ToDictionary(p => p.PriceDate);
        var atrByDate = atrSeries.ToDictionary(p => p.PriceDate);
        var cciByDate = cciSeries.ToDictionary(p => p.PriceDate);
        var chaikinMoneyFlowByDate = chaikinMoneyFlowSeries.ToDictionary(p => p.PriceDate);
        var keltnerChannelsByDate = keltnerChannelsSeries.ToDictionary(p => p.PriceDate);

        var commonDates = macdByDate.Keys
            .Intersect(rsiByDate.Keys)
            .Intersect(bollingerByDate.Keys)
            .Intersect(adxByDate.Keys)
            .Intersect(obvByDate.Keys)
            .Intersect(atrByDate.Keys)
            .Intersect(cciByDate.Keys)
            .Intersect(chaikinMoneyFlowByDate.Keys)
            .Intersect(keltnerChannelsByDate.Keys)
            .OrderBy(date => date)
            .ToList();

        if (commonDates.Count == 0)
        {
            throw new ArgumentException("The supplied indicator series share no common PriceDate; each calculator's warm-up period may not overlap.");
        }

        var points = new List<TradeSignalPoint>(commonDates.Count);

        foreach (var date in commonDates)
        {
            var macd = macdByDate[date];
            var rsi = rsiByDate[date];
            var bollinger = bollingerByDate[date];
            var adx = adxByDate[date];
            var obv = obvByDate[date];
            var atr = atrByDate[date];
            var cci = cciByDate[date];
            var chaikinMoneyFlow = chaikinMoneyFlowByDate[date];
            var keltnerChannels = keltnerChannelsByDate[date];

            var (action, regime, confidence) = Evaluate(macd, rsi, bollinger, adx, obv, cci, chaikinMoneyFlow, keltnerChannels);

            decimal? stopLossPrice = null;
            if (action != TradeAction.Hold && pricesByDate.TryGetValue(date, out var closePrice))
            {
                var atrDistance = atr.Value * AtrStopLossMultiplier;
                stopLossPrice = action == TradeAction.Buy
                    ? closePrice - atrDistance
                    : closePrice + atrDistance;
            }

            points.Add(new TradeSignalPoint(macd.TickerSymbol, date, action, regime, confidence, atr.Value, stopLossPrice));
        }

        return points;
    }

    /// <summary>
    /// Applies the regime-switching rules for a single aligned point across all indicators.
    /// </summary>
    private static (TradeAction Action, MarketRegime Regime, decimal Confidence) Evaluate(
        MacdPoint macd, RsiPoint rsi, BollingerBandsPoint bollinger, AdxPoint adx, ObvPoint obv,
        CciPoint cci, ChaikinMoneyFlowPoint chaikinMoneyFlow, KeltnerChannelsPoint keltnerChannels)
    {
        if (adx.TrendStrength == AdxTrendStrength.Strong)
        {
            return macd.Crossover switch
            {
                MacdCrossover.Bullish => (TradeAction.Buy, MarketRegime.Trending,
                    TrendingConfidence(
                        volumeConfirms: obv.Trend == ObvTrend.Bullish,
                        volatilityExpansionConfirms: keltnerChannels.Signal == KeltnerChannelSignal.AboveUpperBand)),
                MacdCrossover.Bearish => (TradeAction.Sell, MarketRegime.Trending,
                    TrendingConfidence(
                        volumeConfirms: obv.Trend == ObvTrend.Bearish,
                        volatilityExpansionConfirms: keltnerChannels.Signal == KeltnerChannelSignal.BelowLowerBand)),
                _ => (TradeAction.Hold, MarketRegime.Trending, NoConfidence)
            };
        }

        if (bollinger.Signal == BollingerBandSignal.BelowLowerBand && rsi.Zone == RsiZone.Oversold)
        {
            // Money flow still running negative under a Buy setup means the decline is
            // still being distributed into, not accumulated — a classic "falling knife".
            // That's independent information a price oscillator can't see, so it vetoes
            // the trade rather than merely lowering its confidence.
            if (chaikinMoneyFlow.Zone == ChaikinMoneyFlowZone.Bearish)
            {
                return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
            }

            return (TradeAction.Buy, MarketRegime.Ranging, cci.Zone == CciZone.Oversold ? FullConfidence : ModerateConfidence);
        }

        if (bollinger.Signal == BollingerBandSignal.AboveUpperBand && rsi.Zone == RsiZone.Overbought)
        {
            // Symmetric veto: heavy buying pressure under a Sell setup suggests the rally
            // is still being accumulated into, not distributed.
            if (chaikinMoneyFlow.Zone == ChaikinMoneyFlowZone.Bullish)
            {
                return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
            }

            return (TradeAction.Sell, MarketRegime.Ranging, cci.Zone == CciZone.Overbought ? FullConfidence : ModerateConfidence);
        }

        return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
    }

    /// <summary>
    /// Confidence for a Trending-regime MACD crossover, layered from two independent
    /// confirmations: <paramref name="volumeConfirms"/> (OBV agrees with the crossover
    /// direction) and <paramref name="volatilityExpansionConfirms"/> (price has broken the
    /// same-direction Keltner band, indicating real volatility expansion behind the move
    /// rather than a weak EMA cross). Both agreeing is the strongest read; the MACD
    /// crossover alone, with neither confirming, is still enough to signal but at the
    /// lowest confidence.
    /// </summary>
    private static decimal TrendingConfidence(bool volumeConfirms, bool volatilityExpansionConfirms)
    {
        return (volumeConfirms, volatilityExpansionConfirms) switch
        {
            (true, true) => FullConfidence,
            (true, false) => HighConfidence,
            (false, true) => ModerateConfidence,
            (false, false) => LowConfidence
        };
    }

    private static IReadOnlyList<T> RequireNonEmpty<T>(IReadOnlyList<T>? series, string paramName)
    {
        if (series is null || series.Count == 0)
        {
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        }

        return series;
    }
}
