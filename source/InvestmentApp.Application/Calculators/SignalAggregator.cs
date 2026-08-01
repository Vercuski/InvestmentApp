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
/// A bullish/bearish MACD crossover becomes a Buy/Sell. Confidence is then scored by how
/// many of six independent trend confirmations agree with that direction: <see cref="ObvPoint.Trend"/>
/// (volume behind the move), price breaking the same-direction <see cref="KeltnerChannelsPoint"/>
/// band (genuine volatility expansion), <see cref="SuperTrendPoint.Trend"/>,
/// <see cref="ParabolicSarPoint.Trend"/>, a same-direction <see cref="DonchianChannelsPoint.Signal"/>
/// breakout, and a same-direction <see cref="AroonPoint.Trend"/>. See
/// <see cref="TrendingConfidence"/> for how the count of agreeing confirmations maps to a tier.
/// </description></item>
/// <item><description>
/// <b>Ranging</b> (<see cref="AdxTrendStrength.Weak"/>): trade mean-reversion. Price at the
/// lower Bollinger Band together with an oversold RSI is a Buy; price at the upper band
/// together with an overbought RSI is a Sell. MACD crossovers are ignored in this regime,
/// since trend-following signals whipsaw in a ranging market. Confidence is then scored by
/// how many of three independent oscillators agree with the same overbought/oversold read:
/// <see cref="CciPoint.Zone"/>, <see cref="MfiPoint.Zone"/>, and <see cref="WilliamsPercentRPoint.Zone"/>
/// &#8212; three different constructions (a mean-deviation oscillator, a volume-weighted
/// oscillator, and a pure range oscillator) landing on the same read is stronger evidence
/// than any one alone. See <see cref="RangingConfidence"/> for how the count maps to a tier.
/// <see cref="ChaikinMoneyFlowPoint.Zone"/> still acts as a veto rather than a vote: money
/// flow running against the reversal (heavy distribution under a Buy setup, heavy
/// accumulation under a Sell setup) is a "falling knife" warning that suppresses the signal
/// entirely, since it's independent information the price oscillators above can't see.
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
    // Confidence tiers. Kept as named constants rather than inline literals so the meaning
    // of each number stays legible at the call site in Evaluate(), TrendingConfidence(),
    // and RangingConfidence().
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
    /// (an inner join), since each calculator becomes defined after a different number of
    /// warm-up bars.
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
        IReadOnlyList<KeltnerChannelsPoint> keltnerChannelsSeries,
        IReadOnlyList<SuperTrendPoint> superTrendSeries,
        IReadOnlyList<ParabolicSarPoint> parabolicSarSeries,
        IReadOnlyList<DonchianChannelsPoint> donchianChannelsSeries,
        IReadOnlyList<MfiPoint> mfiSeries,
        IReadOnlyList<WilliamsPercentRPoint> williamsPercentRSeries,
        IReadOnlyList<AroonPoint> aroonSeries)
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
        RequireNonEmpty(superTrendSeries, nameof(superTrendSeries));
        RequireNonEmpty(parabolicSarSeries, nameof(parabolicSarSeries));
        RequireNonEmpty(donchianChannelsSeries, nameof(donchianChannelsSeries));
        RequireNonEmpty(mfiSeries, nameof(mfiSeries));
        RequireNonEmpty(williamsPercentRSeries, nameof(williamsPercentRSeries));
        RequireNonEmpty(aroonSeries, nameof(aroonSeries));

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
        var superTrendByDate = superTrendSeries.ToDictionary(p => p.PriceDate);
        var parabolicSarByDate = parabolicSarSeries.ToDictionary(p => p.PriceDate);
        var donchianChannelsByDate = donchianChannelsSeries.ToDictionary(p => p.PriceDate);
        var mfiByDate = mfiSeries.ToDictionary(p => p.PriceDate);
        var williamsPercentRByDate = williamsPercentRSeries.ToDictionary(p => p.PriceDate);
        var aroonByDate = aroonSeries.ToDictionary(p => p.PriceDate);

        var commonDates = macdByDate.Keys
            .Intersect(rsiByDate.Keys)
            .Intersect(bollingerByDate.Keys)
            .Intersect(adxByDate.Keys)
            .Intersect(obvByDate.Keys)
            .Intersect(atrByDate.Keys)
            .Intersect(cciByDate.Keys)
            .Intersect(chaikinMoneyFlowByDate.Keys)
            .Intersect(keltnerChannelsByDate.Keys)
            .Intersect(superTrendByDate.Keys)
            .Intersect(parabolicSarByDate.Keys)
            .Intersect(donchianChannelsByDate.Keys)
            .Intersect(mfiByDate.Keys)
            .Intersect(williamsPercentRByDate.Keys)
            .Intersect(aroonByDate.Keys)
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
            var superTrend = superTrendByDate[date];
            var parabolicSar = parabolicSarByDate[date];
            var donchianChannels = donchianChannelsByDate[date];
            var mfi = mfiByDate[date];
            var williamsPercentR = williamsPercentRByDate[date];
            var aroon = aroonByDate[date];

            var (action, regime, confidence) = Evaluate(
                macd, rsi, bollinger, adx, obv, cci, chaikinMoneyFlow, keltnerChannels,
                superTrend, parabolicSar, donchianChannels, mfi, williamsPercentR, aroon);

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
        CciPoint cci, ChaikinMoneyFlowPoint chaikinMoneyFlow, KeltnerChannelsPoint keltnerChannels,
        SuperTrendPoint superTrend, ParabolicSarPoint parabolicSar, DonchianChannelsPoint donchianChannels,
        MfiPoint mfi, WilliamsPercentRPoint williamsPercentR, AroonPoint aroon)
    {
        if (adx.TrendStrength == AdxTrendStrength.Strong)
        {
            return macd.Crossover switch
            {
                MacdCrossover.Bullish => (TradeAction.Buy, MarketRegime.Trending,
                    TrendingConfidence(
                        volumeConfirms: obv.Trend == ObvTrend.Bullish,
                        volatilityExpansionConfirms: keltnerChannels.Signal == KeltnerChannelSignal.AboveUpperBand,
                        superTrendConfirms: superTrend.Trend == SuperTrendTrend.Bullish,
                        parabolicSarConfirms: parabolicSar.Trend == ParabolicSarTrend.Bullish,
                        donchianConfirms: donchianChannels.Signal == DonchianChannelSignal.AboveUpperBand,
                        aroonConfirms: aroon.Trend == AroonTrend.Uptrend)),
                MacdCrossover.Bearish => (TradeAction.Sell, MarketRegime.Trending,
                    TrendingConfidence(
                        volumeConfirms: obv.Trend == ObvTrend.Bearish,
                        volatilityExpansionConfirms: keltnerChannels.Signal == KeltnerChannelSignal.BelowLowerBand,
                        superTrendConfirms: superTrend.Trend == SuperTrendTrend.Bearish,
                        parabolicSarConfirms: parabolicSar.Trend == ParabolicSarTrend.Bearish,
                        donchianConfirms: donchianChannels.Signal == DonchianChannelSignal.BelowLowerBand,
                        aroonConfirms: aroon.Trend == AroonTrend.Downtrend)),
                _ => (TradeAction.Hold, MarketRegime.Trending, NoConfidence)
            };
        }

        if (bollinger.Signal == BollingerBandSignal.BelowLowerBand && rsi.Zone == RsiZone.Oversold)
        {
            // Money flow still running negative under a Buy setup means the decline is
            // still being distributed into, not accumulated — a classic "falling knife".
            // That's independent information the oscillators below can't see, so it vetoes
            // the trade rather than merely lowering its confidence.
            if (chaikinMoneyFlow.Zone == ChaikinMoneyFlowZone.Bearish)
            {
                return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
            }

            return (TradeAction.Buy, MarketRegime.Ranging,
                RangingConfidence(
                    cciConfirms: cci.Zone == CciZone.Oversold,
                    mfiConfirms: mfi.Zone == MfiZone.Oversold,
                    williamsPercentRConfirms: williamsPercentR.Zone == WilliamsPercentRZone.Oversold));
        }

        if (bollinger.Signal == BollingerBandSignal.AboveUpperBand && rsi.Zone == RsiZone.Overbought)
        {
            // Symmetric veto: heavy buying pressure under a Sell setup suggests the rally
            // is still being accumulated into, not distributed.
            if (chaikinMoneyFlow.Zone == ChaikinMoneyFlowZone.Bullish)
            {
                return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
            }

            return (TradeAction.Sell, MarketRegime.Ranging,
                RangingConfidence(
                    cciConfirms: cci.Zone == CciZone.Overbought,
                    mfiConfirms: mfi.Zone == MfiZone.Overbought,
                    williamsPercentRConfirms: williamsPercentR.Zone == WilliamsPercentRZone.Overbought));
        }

        return (TradeAction.Hold, MarketRegime.Ranging, NoConfidence);
    }

    /// <summary>
    /// Confidence for a Trending-regime MACD crossover, scored by how many of six
    /// independent confirmations agree with the crossover's direction: volume (OBV),
    /// volatility expansion (Keltner Channels), and the directional state of SuperTrend,
    /// Parabolic SAR, Donchian Channels, and Aroon. The MACD crossover alone, with nothing
    /// confirming, is still enough to signal but at the lowest confidence; agreement from
    /// most or all of the other six is the strongest read.
    /// </summary>
    /// <remarks>
    /// The thresholds (5+ of 6 for Full, 3+ for High, 1+ for Moderate) are starting
    /// judgment calls, same as the regime thresholds on <see cref="AdxCalculator"/> itself,
    /// and may need tuning against real trading results.
    /// </remarks>
    private static decimal TrendingConfidence(
        bool volumeConfirms,
        bool volatilityExpansionConfirms,
        bool superTrendConfirms,
        bool parabolicSarConfirms,
        bool donchianConfirms,
        bool aroonConfirms)
    {
        int agreeing = CountTrue(
            volumeConfirms, volatilityExpansionConfirms, superTrendConfirms,
            parabolicSarConfirms, donchianConfirms, aroonConfirms);

        return agreeing switch
        {
            >= 5 => FullConfidence,
            >= 3 => HighConfidence,
            >= 1 => ModerateConfidence,
            _ => LowConfidence
        };
    }

    /// <summary>
    /// Confidence for a Ranging-regime Bollinger+RSI setup (after the CMF veto has already
    /// been checked), scored by how many of three independent oscillators agree with the
    /// same overbought/oversold read: CCI, MFI, and Williams %R. All three agreeing is the
    /// strongest read; the bare Bollinger+RSI setup, with none of them confirming, is still
    /// enough to signal at a moderate confidence, matching this regime's original behavior
    /// before CCI was the only available confirmation.
    /// </summary>
    /// <remarks>
    /// The thresholds (3 of 3 for Full, 2 of 3 for High, otherwise Moderate) are starting
    /// judgment calls and may need tuning against real trading results.
    /// </remarks>
    private static decimal RangingConfidence(bool cciConfirms, bool mfiConfirms, bool williamsPercentRConfirms)
    {
        int agreeing = CountTrue(cciConfirms, mfiConfirms, williamsPercentRConfirms);

        return agreeing switch
        {
            3 => FullConfidence,
            2 => HighConfidence,
            _ => ModerateConfidence
        };
    }

    private static int CountTrue(params bool[] confirmations)
    {
        int count = 0;
        foreach (var confirms in confirmations)
        {
            if (confirms)
            {
                count++;
            }
        }
        return count;
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
