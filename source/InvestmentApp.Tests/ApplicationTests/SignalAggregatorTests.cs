using InvestmentApp.Application.Calculators;
using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Tests.ApplicationTests;

/// <summary>
/// Characterization tests for <see cref="SignalAggregator"/>. These exercise the class purely
/// through its public API (constructor + <see cref="SignalAggregator.Calculate"/>) rather than
/// via reflection into its private helpers, so every test doubles as documentation of the
/// regime-switching contract described in the class's own XML remarks.
/// </summary>
[TestFixture]
public class SignalAggregatorTests
{
    // Mirrors the private confidence-tier constants declared on SignalAggregator itself.
    // Kept here (rather than referenced directly) so these tests fail loudly if the
    // aggregator's own tier values ever change.
    private const decimal ExpectedFullConfidence = 1.0m;
    private const decimal ExpectedHighConfidence = 0.85m;
    private const decimal ExpectedModerateConfidence = 0.75m;
    private const decimal ExpectedLowConfidence = 0.6m;
    private const decimal ExpectedNoConfidence = 0m;

    private const string Ticker = "TEST";
    private static readonly DateTime BaseDate = new(2024, 3, 1);

    #region Series factories

    private static IReadOnlyList<StockData> PriceSeries(DateTime date, decimal close = 100m)
    {
        return [new(Ticker, close, close, close, close, 1_000_000, date)];
    }

    private static IReadOnlyList<MacdPoint> MacdSeries(DateTime date, MacdCrossover crossover = MacdCrossover.None)
    {
        return [new(Ticker, date, 0m, 0m, 0m, crossover)];
    }

    private static IReadOnlyList<RsiPoint> RsiSeries(DateTime date, RsiZone zone = RsiZone.Neutral)
    {
        return [new(Ticker, date, 50m, zone)];
    }

    private static IReadOnlyList<BollingerBandsPoint> BollingerSeries(DateTime date, BollingerBandSignal signal = BollingerBandSignal.WithinBands)
    {
        return [new(Ticker, date, 100m, 100m, 110m, 90m, signal)];
    }

    private static IReadOnlyList<AdxPoint> AdxSeries(DateTime date, AdxTrendStrength strength = AdxTrendStrength.Weak)
    {
        return [new(Ticker, date, 20m, 15m, 15m, strength)];
    }

    private static IReadOnlyList<ObvPoint> ObvSeries(DateTime date, ObvTrend trend = ObvTrend.None)
    {
        return [new(Ticker, date, 1_000m, 1_000m, trend)];
    }

    private static IReadOnlyList<AtrPoint> AtrSeries(DateTime date, decimal value = 2m)
    {
        return [new(Ticker, date, value)];
    }

    private static IReadOnlyList<CciPoint> CciSeries(DateTime date, CciZone zone = CciZone.Neutral)
    {
        return [new(Ticker, date, 0m, zone)];
    }

    private static IReadOnlyList<ChaikinMoneyFlowPoint> CmfSeries(DateTime date, ChaikinMoneyFlowZone zone = ChaikinMoneyFlowZone.Neutral)
    {
        return [new(Ticker, date, 0m, zone)];
    }

    private static IReadOnlyList<KeltnerChannelsPoint> KeltnerSeries(DateTime date, KeltnerChannelSignal signal = KeltnerChannelSignal.WithinChannel)
    {
        return [new(Ticker, date, 100m, 100m, 110m, 90m, signal)];
    }

    private static IReadOnlyList<SuperTrendPoint> SuperTrendSeries(DateTime date, SuperTrendTrend trend = SuperTrendTrend.Bullish)
    {
        return [new(Ticker, date, 100m, trend)];
    }

    private static IReadOnlyList<ParabolicSarPoint> ParabolicSarSeries(DateTime date, ParabolicSarTrend trend = ParabolicSarTrend.Bullish)
    {
        return [new(Ticker, date, 100m, trend)];
    }

    private static IReadOnlyList<DonchianChannelsPoint> DonchianSeries(DateTime date, DonchianChannelSignal signal = DonchianChannelSignal.WithinChannel)
    {
        return [new(Ticker, date, 100m, 110m, 100m, 90m, signal)];
    }

    private static IReadOnlyList<MfiPoint> MfiSeries(DateTime date, MfiZone zone = MfiZone.Neutral)
    {
        return [new(Ticker, date, 50m, zone)];
    }

    private static IReadOnlyList<WilliamsPercentRPoint> WilliamsPercentRSeries(DateTime date, WilliamsPercentRZone zone = WilliamsPercentRZone.Neutral)
    {
        return [new(Ticker, date, -50m, zone)];
    }

    private static IReadOnlyList<AroonPoint> AroonSeries(DateTime date, AroonTrend trend = AroonTrend.Neutral)
    {
        return [new(Ticker, date, 50m, 50m, 0m, trend)];
    }

    private static List<T> Concat<T>(params IReadOnlyList<T>[] lists)
    {
        return [.. lists.SelectMany(l => l)];
    }

    #endregion

    #region Scenario helpers

    /// <summary>
    /// Runs the aggregator for a single aligned date (<see cref="BaseDate"/>), overriding only
    /// the indicator fields a given test cares about. Every parameter defaults to a "neutral,
    /// no signal" reading, so an unmodified call is expected to resolve to Hold.
    /// </summary>
    private static TradeSignalPoint CalculateSingle(
        SignalAggregator aggregator,
        decimal closePrice = 100m,
        MacdCrossover macdCrossover = MacdCrossover.None,
        RsiZone rsiZone = RsiZone.Neutral,
        BollingerBandSignal bollingerSignal = BollingerBandSignal.WithinBands,
        AdxTrendStrength adxStrength = AdxTrendStrength.Weak,
        ObvTrend obvTrend = ObvTrend.None,
        decimal atrValue = 2m,
        CciZone cciZone = CciZone.Neutral,
        ChaikinMoneyFlowZone cmfZone = ChaikinMoneyFlowZone.Neutral,
        KeltnerChannelSignal keltnerSignal = KeltnerChannelSignal.WithinChannel,
        SuperTrendTrend superTrendTrend = SuperTrendTrend.Bullish,
        ParabolicSarTrend parabolicSarTrend = ParabolicSarTrend.Bullish,
        DonchianChannelSignal donchianSignal = DonchianChannelSignal.WithinChannel,
        MfiZone mfiZone = MfiZone.Neutral,
        WilliamsPercentRZone williamsPercentRZone = WilliamsPercentRZone.Neutral,
        AroonTrend aroonTrend = AroonTrend.Neutral)
    {
        var result = aggregator.Calculate(
            PriceSeries(BaseDate, closePrice),
            MacdSeries(BaseDate, macdCrossover),
            RsiSeries(BaseDate, rsiZone),
            BollingerSeries(BaseDate, bollingerSignal),
            AdxSeries(BaseDate, adxStrength),
            ObvSeries(BaseDate, obvTrend),
            AtrSeries(BaseDate, atrValue),
            CciSeries(BaseDate, cciZone),
            CmfSeries(BaseDate, cmfZone),
            KeltnerSeries(BaseDate, keltnerSignal),
            SuperTrendSeries(BaseDate, superTrendTrend),
            ParabolicSarSeries(BaseDate, parabolicSarTrend),
            DonchianSeries(BaseDate, donchianSignal),
            MfiSeries(BaseDate, mfiZone),
            WilliamsPercentRSeries(BaseDate, williamsPercentRZone),
            AroonSeries(BaseDate, aroonTrend));

        return result.Single();
    }

    /// <summary>
    /// Builds a Trending-regime scenario with a given MACD crossover direction and exactly
    /// <paramref name="confirmingCount"/> of the six trend confirmations (OBV, Keltner,
    /// SuperTrend, Parabolic SAR, Donchian, Aroon) agreeing with that direction.
    /// </summary>
    private static TradeSignalPoint CalculateTrendingSignal(SignalAggregator aggregator, MacdCrossover crossover, int confirmingCount)
    {
        var bullish = crossover == MacdCrossover.Bullish;
        var confirms = new bool[6];
        for (var i = 0; i < confirmingCount; i++)
        {
            confirms[i] = true;
        }

        return CalculateSingle(
            aggregator,
            macdCrossover: crossover,
            adxStrength: AdxTrendStrength.Strong,
            obvTrend: confirms[0]
                ? (bullish ? ObvTrend.Bullish : ObvTrend.Bearish)
                : (bullish ? ObvTrend.Bearish : ObvTrend.Bullish),
            keltnerSignal: confirms[1]
                ? (bullish ? KeltnerChannelSignal.AboveUpperBand : KeltnerChannelSignal.BelowLowerBand)
                : KeltnerChannelSignal.WithinChannel,
            superTrendTrend: confirms[2]
                ? (bullish ? SuperTrendTrend.Bullish : SuperTrendTrend.Bearish)
                : (bullish ? SuperTrendTrend.Bearish : SuperTrendTrend.Bullish),
            parabolicSarTrend: confirms[3]
                ? (bullish ? ParabolicSarTrend.Bullish : ParabolicSarTrend.Bearish)
                : (bullish ? ParabolicSarTrend.Bearish : ParabolicSarTrend.Bullish),
            donchianSignal: confirms[4]
                ? (bullish ? DonchianChannelSignal.AboveUpperBand : DonchianChannelSignal.BelowLowerBand)
                : DonchianChannelSignal.WithinChannel,
            aroonTrend: confirms[5]
                ? (bullish ? AroonTrend.Uptrend : AroonTrend.Downtrend)
                : AroonTrend.Neutral);
    }

    /// <summary>
    /// Builds a Ranging-regime Bollinger+RSI setup (Buy when <paramref name="buySetup"/>,
    /// otherwise Sell) with exactly <paramref name="confirmingCount"/> of the three oscillator
    /// confirmations (CCI, MFI, Williams %R) agreeing, and a given CMF veto zone.
    /// </summary>
    private static TradeSignalPoint CalculateRangingSignal(SignalAggregator aggregator, bool buySetup, int confirmingCount, ChaikinMoneyFlowZone cmfZone)
    {
        var confirms = new bool[3];
        for (var i = 0; i < confirmingCount; i++)
        {
            confirms[i] = true;
        }

        return CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Weak,
            bollingerSignal: buySetup ? BollingerBandSignal.BelowLowerBand : BollingerBandSignal.AboveUpperBand,
            rsiZone: buySetup ? RsiZone.Oversold : RsiZone.Overbought,
            cmfZone: cmfZone,
            cciZone: confirms[0] ? (buySetup ? CciZone.Oversold : CciZone.Overbought) : CciZone.Neutral,
            mfiZone: confirms[1] ? (buySetup ? MfiZone.Oversold : MfiZone.Overbought) : MfiZone.Neutral,
            williamsPercentRZone: confirms[2] ? (buySetup ? WilliamsPercentRZone.Oversold : WilliamsPercentRZone.Overbought) : WilliamsPercentRZone.Neutral);
    }

    /// <summary>
    /// Holds one valid, non-empty, mutually-aligned series per <see cref="SignalAggregator.Calculate"/>
    /// parameter. Validation tests null out or empty exactly one property and invoke, leaving
    /// every other argument valid so the failure can only come from the property under test.
    /// </summary>
    private sealed class ValidCalculateArgs
    {
        public IReadOnlyList<StockData>? Prices { get; set; } = PriceSeries(BaseDate);
        public IReadOnlyList<MacdPoint>? Macd { get; set; } = MacdSeries(BaseDate);
        public IReadOnlyList<RsiPoint>? Rsi { get; set; } = RsiSeries(BaseDate);
        public IReadOnlyList<BollingerBandsPoint>? Bollinger { get; set; } = BollingerSeries(BaseDate);
        public IReadOnlyList<AdxPoint>? Adx { get; set; } = AdxSeries(BaseDate);
        public IReadOnlyList<ObvPoint>? Obv { get; set; } = ObvSeries(BaseDate);
        public IReadOnlyList<AtrPoint>? Atr { get; set; } = AtrSeries(BaseDate);
        public IReadOnlyList<CciPoint>? Cci { get; set; } = CciSeries(BaseDate);
        public IReadOnlyList<ChaikinMoneyFlowPoint>? Cmf { get; set; } = CmfSeries(BaseDate);
        public IReadOnlyList<KeltnerChannelsPoint>? Keltner { get; set; } = KeltnerSeries(BaseDate);
        public IReadOnlyList<SuperTrendPoint>? SuperTrend { get; set; } = SuperTrendSeries(BaseDate);
        public IReadOnlyList<ParabolicSarPoint>? ParabolicSar { get; set; } = ParabolicSarSeries(BaseDate);
        public IReadOnlyList<DonchianChannelsPoint>? Donchian { get; set; } = DonchianSeries(BaseDate);
        public IReadOnlyList<MfiPoint>? Mfi { get; set; } = MfiSeries(BaseDate);
        public IReadOnlyList<WilliamsPercentRPoint>? WilliamsPercentR { get; set; } = WilliamsPercentRSeries(BaseDate);
        public IReadOnlyList<AroonPoint>? Aroon { get; set; } = AroonSeries(BaseDate);

        public IReadOnlyList<TradeSignalPoint> Invoke(SignalAggregator aggregator)
        {
            return aggregator.Calculate(
                Prices!, Macd!, Rsi!, Bollinger!, Adx!, Obv!, Atr!, Cci!, Cmf!, Keltner!,
                SuperTrend!, ParabolicSar!, Donchian!, Mfi!, WilliamsPercentR!, Aroon!);
        }
    }

    private static void AssertThrowsForMissingSeries(Action<ValidCalculateArgs> makeInvalid, string expectedParamName)
    {
        var args = new ValidCalculateArgs();
        makeInvalid(args);

        var ex = Assert.Throws<ArgumentException>(() => args.Invoke(new SignalAggregator()));
        Assert.That(ex!.ParamName, Is.EqualTo(expectedParamName));
    }

    #endregion

    #region Constructor

    [Test]
    public void Constructor_WithNoArguments_DefaultsMultiplierToTwo()
    {
        var aggregator = new SignalAggregator();
        Assert.That(aggregator.AtrStopLossMultiplier, Is.EqualTo(2m));
    }

    private static IEnumerable<TestCaseData> PositiveMultipliers()
    {
        yield return new TestCaseData(0.5m);
        yield return new TestCaseData(1m);
        yield return new TestCaseData(5m);
    }

    [TestCaseSource(nameof(PositiveMultipliers))]
    public void Constructor_WithPositiveMultiplier_IsAccepted(decimal multiplier)
    {
        var aggregator = new SignalAggregator(multiplier);
        Assert.That(aggregator.AtrStopLossMultiplier, Is.EqualTo(multiplier));
    }

    private static IEnumerable<TestCaseData> NonPositiveMultipliers()
    {
        yield return new TestCaseData(0m);
        yield return new TestCaseData(-1m);
        yield return new TestCaseData(-0.01m);
    }

    [TestCaseSource(nameof(NonPositiveMultipliers))]
    public void Constructor_WithNonPositiveMultiplier_Throws(decimal multiplier)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new SignalAggregator(multiplier));
        Assert.That(ex!.ParamName, Is.EqualTo("atrStopLossMultiplier"));
    }

    #endregion

    #region Calculate argument validation

    [Test]
    public void Calculate_Throws_WhenPriceSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Prices = null, "priceSeries");
        AssertThrowsForMissingSeries(a => a.Prices = [], "priceSeries");
    }

    [Test]
    public void Calculate_Throws_WhenMacdSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Macd = null, "macdSeries");
        AssertThrowsForMissingSeries(a => a.Macd = [], "macdSeries");
    }

    [Test]
    public void Calculate_Throws_WhenRsiSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Rsi = null, "rsiSeries");
        AssertThrowsForMissingSeries(a => a.Rsi = [], "rsiSeries");
    }

    [Test]
    public void Calculate_Throws_WhenBollingerBandsSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Bollinger = null, "bollingerBandsSeries");
        AssertThrowsForMissingSeries(a => a.Bollinger = [], "bollingerBandsSeries");
    }

    [Test]
    public void Calculate_Throws_WhenAdxSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Adx = null, "adxSeries");
        AssertThrowsForMissingSeries(a => a.Adx = [], "adxSeries");
    }

    [Test]
    public void Calculate_Throws_WhenObvSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Obv = null, "obvSeries");
        AssertThrowsForMissingSeries(a => a.Obv = [], "obvSeries");
    }

    [Test]
    public void Calculate_Throws_WhenAtrSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Atr = null, "atrSeries");
        AssertThrowsForMissingSeries(a => a.Atr = [], "atrSeries");
    }

    [Test]
    public void Calculate_Throws_WhenCciSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Cci = null, "cciSeries");
        AssertThrowsForMissingSeries(a => a.Cci = [], "cciSeries");
    }

    [Test]
    public void Calculate_Throws_WhenChaikinMoneyFlowSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Cmf = null, "chaikinMoneyFlowSeries");
        AssertThrowsForMissingSeries(a => a.Cmf = [], "chaikinMoneyFlowSeries");
    }

    [Test]
    public void Calculate_Throws_WhenKeltnerChannelsSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Keltner = null, "keltnerChannelsSeries");
        AssertThrowsForMissingSeries(a => a.Keltner = [], "keltnerChannelsSeries");
    }

    [Test]
    public void Calculate_Throws_WhenSuperTrendSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.SuperTrend = null, "superTrendSeries");
        AssertThrowsForMissingSeries(a => a.SuperTrend = [], "superTrendSeries");
    }

    [Test]
    public void Calculate_Throws_WhenParabolicSarSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.ParabolicSar = null, "parabolicSarSeries");
        AssertThrowsForMissingSeries(a => a.ParabolicSar = [], "parabolicSarSeries");
    }

    [Test]
    public void Calculate_Throws_WhenDonchianChannelsSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Donchian = null, "donchianChannelsSeries");
        AssertThrowsForMissingSeries(a => a.Donchian = [], "donchianChannelsSeries");
    }

    [Test]
    public void Calculate_Throws_WhenMfiSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Mfi = null, "mfiSeries");
        AssertThrowsForMissingSeries(a => a.Mfi = [], "mfiSeries");
    }

    [Test]
    public void Calculate_Throws_WhenWilliamsPercentRSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.WilliamsPercentR = null, "williamsPercentRSeries");
        AssertThrowsForMissingSeries(a => a.WilliamsPercentR = [], "williamsPercentRSeries");
    }

    [Test]
    public void Calculate_Throws_WhenAroonSeriesIsMissing()
    {
        AssertThrowsForMissingSeries(a => a.Aroon = null, "aroonSeries");
        AssertThrowsForMissingSeries(a => a.Aroon = [], "aroonSeries");
    }

    [Test]
    public void Calculate_Throws_WhenIndicatorSeriesShareNoCommonDate()
    {
        var dateA = new DateTime(2024, 1, 1);
        var dateB = new DateTime(2024, 6, 1);
        var aggregator = new SignalAggregator();

        var ex = Assert.Throws<ArgumentException>(() => aggregator.Calculate(
            PriceSeries(dateA),
            MacdSeries(dateA),
            RsiSeries(dateB), // deliberately misaligned with every other series
            BollingerSeries(dateA),
            AdxSeries(dateA),
            ObvSeries(dateA),
            AtrSeries(dateA),
            CciSeries(dateA),
            CmfSeries(dateA),
            KeltnerSeries(dateA),
            SuperTrendSeries(dateA),
            ParabolicSarSeries(dateA),
            DonchianSeries(dateA),
            MfiSeries(dateA),
            WilliamsPercentRSeries(dateA),
            AroonSeries(dateA)));

        Assert.That(ex!.Message, Does.Contain("share no common PriceDate"));
    }

    #endregion

    #region Baseline sanity

    [Test]
    public void Calculate_DefaultNeutralScenario_ProducesHoldWithNoConfidence()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(aggregator);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Ranging));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedNoConfidence));
        }
    }

    #endregion

    #region Trending regime

    [Test]
    public void Calculate_StrongAdxWithBullishMacdCrossover_ProducesBuyInTrendingRegime()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateTrendingSignal(aggregator, MacdCrossover.Bullish, confirmingCount: 0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Buy));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Trending));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedLowConfidence));
        }
    }

    [Test]
    public void Calculate_StrongAdxWithBearishMacdCrossover_ProducesSellInTrendingRegime()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateTrendingSignal(aggregator, MacdCrossover.Bearish, confirmingCount: 0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Sell));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Trending));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedLowConfidence));
        }
    }

    [Test]
    public void Calculate_StrongAdxWithNoMacdCrossover_ProducesHoldInTrendingRegime()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(aggregator, adxStrength: AdxTrendStrength.Strong, macdCrossover: MacdCrossover.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Trending));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedNoConfidence));
        }
    }

    [Test]
    public void Calculate_StrongAdx_IgnoresRangingSetup_EvenWhenBollingerAndRsiIndicateAReversal()
    {
        // A Buy-like Bollinger+RSI+oscillator setup should be irrelevant while ADX reports a
        // strong trend: only the MACD crossover drives the action in that regime.
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Strong,
            macdCrossover: MacdCrossover.None,
            bollingerSignal: BollingerBandSignal.BelowLowerBand,
            rsiZone: RsiZone.Oversold,
            cmfZone: ChaikinMoneyFlowZone.Neutral,
            cciZone: CciZone.Oversold,
            mfiZone: MfiZone.Oversold,
            williamsPercentRZone: WilliamsPercentRZone.Oversold);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Trending));
        }
    }

    private static IEnumerable<TestCaseData> TrendingConfidenceCases()
    {
        yield return new TestCaseData(0, ExpectedLowConfidence).SetName("{m}(ZeroConfirmations)");
        yield return new TestCaseData(1, ExpectedModerateConfidence).SetName("{m}(OneConfirmation)");
        yield return new TestCaseData(2, ExpectedModerateConfidence).SetName("{m}(TwoConfirmations)");
        yield return new TestCaseData(3, ExpectedHighConfidence).SetName("{m}(ThreeConfirmations)");
        yield return new TestCaseData(4, ExpectedHighConfidence).SetName("{m}(FourConfirmations)");
        yield return new TestCaseData(5, ExpectedFullConfidence).SetName("{m}(FiveConfirmations)");
        yield return new TestCaseData(6, ExpectedFullConfidence).SetName("{m}(SixConfirmations)");
    }

    [TestCaseSource(nameof(TrendingConfidenceCases))]
    public void Calculate_TrendingBuySignal_ConfidenceTierReflectsAgreeingConfirmations(int confirmingCount, decimal expectedConfidence)
    {
        var aggregator = new SignalAggregator();
        var point = CalculateTrendingSignal(aggregator, MacdCrossover.Bullish, confirmingCount);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Buy));
            Assert.That(point.Confidence, Is.EqualTo(expectedConfidence));
        }
    }

    [TestCaseSource(nameof(TrendingConfidenceCases))]
    public void Calculate_TrendingSellSignal_ConfidenceTierReflectsAgreeingConfirmations(int confirmingCount, decimal expectedConfidence)
    {
        var aggregator = new SignalAggregator();
        var point = CalculateTrendingSignal(aggregator, MacdCrossover.Bearish, confirmingCount);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Sell));
            Assert.That(point.Confidence, Is.EqualTo(expectedConfidence));
        }
    }

    #endregion

    #region Ranging regime

    private static IEnumerable<TestCaseData> RangingConfidenceCases()
    {
        yield return new TestCaseData(0, ExpectedModerateConfidence).SetName("{m}(ZeroConfirmations)");
        yield return new TestCaseData(1, ExpectedModerateConfidence).SetName("{m}(OneConfirmation)");
        yield return new TestCaseData(2, ExpectedHighConfidence).SetName("{m}(TwoConfirmations)");
        yield return new TestCaseData(3, ExpectedFullConfidence).SetName("{m}(ThreeConfirmations)");
    }

    [TestCaseSource(nameof(RangingConfidenceCases))]
    public void Calculate_RangingBuySetup_ConfidenceTierReflectsAgreeingOscillators(int confirmingCount, decimal expectedConfidence)
    {
        var aggregator = new SignalAggregator();
        var point = CalculateRangingSignal(aggregator, buySetup: true, confirmingCount, ChaikinMoneyFlowZone.Neutral);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Buy));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Ranging));
            Assert.That(point.Confidence, Is.EqualTo(expectedConfidence));
        }
    }

    [TestCaseSource(nameof(RangingConfidenceCases))]
    public void Calculate_RangingSellSetup_ConfidenceTierReflectsAgreeingOscillators(int confirmingCount, decimal expectedConfidence)
    {
        var aggregator = new SignalAggregator();
        var point = CalculateRangingSignal(aggregator, buySetup: false, confirmingCount, ChaikinMoneyFlowZone.Neutral);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Sell));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Ranging));
            Assert.That(point.Confidence, Is.EqualTo(expectedConfidence));
        }
    }

    [Test]
    public void Calculate_RangingBuySetup_VetoedByBearishMoneyFlow_EvenWithFullOscillatorAgreement()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateRangingSignal(aggregator, buySetup: true, confirmingCount: 3, ChaikinMoneyFlowZone.Bearish);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Ranging));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedNoConfidence));
        }
    }

    [Test]
    public void Calculate_RangingSellSetup_VetoedByBullishMoneyFlow_EvenWithFullOscillatorAgreement()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateRangingSignal(aggregator, buySetup: false, confirmingCount: 3, ChaikinMoneyFlowZone.Bullish);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Regime, Is.EqualTo(MarketRegime.Ranging));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedNoConfidence));
        }
    }

    [Test]
    public void Calculate_RangingBuySetup_NotVetoedByBullishMoneyFlow()
    {
        // Money flow running *with* the reversal (Bullish, under a Buy setup) is not a veto
        // condition — only Bearish flow under a Buy setup (or Bullish flow under a Sell setup)
        // suppresses the trade.
        var aggregator = new SignalAggregator();
        var point = CalculateRangingSignal(aggregator, buySetup: true, confirmingCount: 0, ChaikinMoneyFlowZone.Bullish);

        Assert.That(point.Action, Is.EqualTo(TradeAction.Buy));
    }

    [Test]
    public void Calculate_RangingRegime_BollingerWithinBandsAndNeutralRsi_ProducesHold()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(aggregator, adxStrength: AdxTrendStrength.Weak);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
            Assert.That(point.Confidence, Is.EqualTo(ExpectedNoConfidence));
        }
    }

    [Test]
    public void Calculate_RangingRegime_BollingerBelowLowerBandWithoutOversoldRsi_ProducesHold()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Weak,
            bollingerSignal: BollingerBandSignal.BelowLowerBand,
            rsiZone: RsiZone.Neutral);

        Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
    }

    [Test]
    public void Calculate_RangingRegime_OversoldRsiWithoutLowerBandBreach_ProducesHold()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Weak,
            bollingerSignal: BollingerBandSignal.WithinBands,
            rsiZone: RsiZone.Oversold);

        Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
    }

    [Test]
    public void Calculate_RangingRegime_BollingerAboveUpperBandWithoutOverboughtRsi_ProducesHold()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Weak,
            bollingerSignal: BollingerBandSignal.AboveUpperBand,
            rsiZone: RsiZone.Neutral);

        Assert.That(point.Action, Is.EqualTo(TradeAction.Hold));
    }

    #endregion

    #region Stop-loss and ATR pass-through

    [Test]
    public void Calculate_BuySignal_StopLossIsClosePriceMinusAtrTimesMultiplier()
    {
        var aggregator = new SignalAggregator(atrStopLossMultiplier: 2m);
        var point = CalculateSingle(
            aggregator,
            closePrice: 150m,
            atrValue: 3m,
            macdCrossover: MacdCrossover.Bullish,
            adxStrength: AdxTrendStrength.Strong);

        Assert.That(point.StopLossPrice, Is.EqualTo(150m - 3m * 2m));
    }

    [Test]
    public void Calculate_SellSignal_StopLossIsClosePricePlusAtrTimesMultiplier()
    {
        var aggregator = new SignalAggregator(atrStopLossMultiplier: 2m);
        var point = CalculateSingle(
            aggregator,
            closePrice: 150m,
            atrValue: 3m,
            macdCrossover: MacdCrossover.Bearish,
            adxStrength: AdxTrendStrength.Strong);

        Assert.That(point.StopLossPrice, Is.EqualTo(150m + 3m * 2m));
    }

    [Test]
    public void Calculate_HoldSignal_StopLossIsNull()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(aggregator, adxStrength: AdxTrendStrength.Strong, macdCrossover: MacdCrossover.None);

        Assert.That(point.StopLossPrice, Is.Null);
    }

    [Test]
    public void Calculate_CustomAtrStopLossMultiplier_IsAppliedToTheStopLossCalculation()
    {
        var aggregator = new SignalAggregator(atrStopLossMultiplier: 3m);
        var point = CalculateSingle(
            aggregator,
            closePrice: 100m,
            atrValue: 2m,
            macdCrossover: MacdCrossover.Bullish,
            adxStrength: AdxTrendStrength.Strong);

        Assert.That(point.StopLossPrice, Is.EqualTo(100m - 2m * 3m));
    }

    [Test]
    public void Calculate_AtrValue_IsCarriedThroughRegardlessOfAction()
    {
        var aggregator = new SignalAggregator();
        var holdPoint = CalculateSingle(
            aggregator,
            adxStrength: AdxTrendStrength.Strong,
            macdCrossover: MacdCrossover.None,
            atrValue: 4.5m);

        Assert.That(holdPoint.AtrValue, Is.EqualTo(4.5m));
    }

    [Test]
    public void Calculate_BuySignal_StopLossIsNull_WhenTheAlignedDateIsMissingFromPriceSeries()
    {
        // The aligned date set comes purely from the 14 indicator series; if that date happens
        // not to appear in priceSeries, there's no close price to derive a stop-loss from.
        var indicatorDate = new DateTime(2024, 4, 1);
        var priceOnlyDate = new DateTime(2024, 4, 2);
        var aggregator = new SignalAggregator();

        var result = aggregator.Calculate(
            PriceSeries(priceOnlyDate),
            MacdSeries(indicatorDate, MacdCrossover.Bullish),
            RsiSeries(indicatorDate),
            BollingerSeries(indicatorDate),
            AdxSeries(indicatorDate, AdxTrendStrength.Strong),
            ObvSeries(indicatorDate),
            AtrSeries(indicatorDate),
            CciSeries(indicatorDate),
            CmfSeries(indicatorDate),
            KeltnerSeries(indicatorDate),
            SuperTrendSeries(indicatorDate),
            ParabolicSarSeries(indicatorDate),
            DonchianSeries(indicatorDate),
            MfiSeries(indicatorDate),
            WilliamsPercentRSeries(indicatorDate),
            AroonSeries(indicatorDate));

        var point = result.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.Action, Is.EqualTo(TradeAction.Buy));
            Assert.That(point.StopLossPrice, Is.Null);
        }
    }

    #endregion

    #region Output field pass-through

    [Test]
    public void Calculate_OutputTickerSymbol_IsTakenFromMacdSeries()
    {
        var aggregator = new SignalAggregator();

        var result = aggregator.Calculate(
            PriceSeries(BaseDate),
            [new("MACD-TICKER", BaseDate, 0m, 0m, 0m, MacdCrossover.None)],
            RsiSeries(BaseDate),
            BollingerSeries(BaseDate),
            AdxSeries(BaseDate),
            ObvSeries(BaseDate),
            AtrSeries(BaseDate),
            CciSeries(BaseDate),
            CmfSeries(BaseDate),
            KeltnerSeries(BaseDate),
            SuperTrendSeries(BaseDate),
            ParabolicSarSeries(BaseDate),
            DonchianSeries(BaseDate),
            MfiSeries(BaseDate),
            WilliamsPercentRSeries(BaseDate),
            AroonSeries(BaseDate));

        Assert.That(result.Single().TickerSymbol, Is.EqualTo("MACD-TICKER"));
    }

    [Test]
    public void Calculate_OutputPriceDate_MatchesTheAlignedCommonDate()
    {
        var aggregator = new SignalAggregator();
        var point = CalculateSingle(aggregator);

        Assert.That(point.PriceDate, Is.EqualTo(BaseDate));
    }

    #endregion

    #region Series alignment

    [Test]
    public void Calculate_ProducesOnePointPerCommonDate_OrderedAscendingRegardlessOfInputOrder()
    {
        var day1 = new DateTime(2024, 5, 1);
        var day2 = new DateTime(2024, 5, 2);
        var day3 = new DateTime(2024, 5, 3);
        var aggregator = new SignalAggregator();

        // Feed every series out of order to confirm the aggregator sorts its output itself.
        var result = aggregator.Calculate(
            Concat(PriceSeries(day3), PriceSeries(day1), PriceSeries(day2)),
            Concat(MacdSeries(day3), MacdSeries(day1), MacdSeries(day2)),
            Concat(RsiSeries(day3), RsiSeries(day1), RsiSeries(day2)),
            Concat(BollingerSeries(day3), BollingerSeries(day1), BollingerSeries(day2)),
            Concat(AdxSeries(day3), AdxSeries(day1), AdxSeries(day2)),
            Concat(ObvSeries(day3), ObvSeries(day1), ObvSeries(day2)),
            Concat(AtrSeries(day3), AtrSeries(day1), AtrSeries(day2)),
            Concat(CciSeries(day3), CciSeries(day1), CciSeries(day2)),
            Concat(CmfSeries(day3), CmfSeries(day1), CmfSeries(day2)),
            Concat(KeltnerSeries(day3), KeltnerSeries(day1), KeltnerSeries(day2)),
            Concat(SuperTrendSeries(day3), SuperTrendSeries(day1), SuperTrendSeries(day2)),
            Concat(ParabolicSarSeries(day3), ParabolicSarSeries(day1), ParabolicSarSeries(day2)),
            Concat(DonchianSeries(day3), DonchianSeries(day1), DonchianSeries(day2)),
            Concat(MfiSeries(day3), MfiSeries(day1), MfiSeries(day2)),
            Concat(WilliamsPercentRSeries(day3), WilliamsPercentRSeries(day1), WilliamsPercentRSeries(day2)),
            Concat(AroonSeries(day3), AroonSeries(day1), AroonSeries(day2)));

        Assert.That(result.Select(p => p.PriceDate), Is.EqualTo([day1, day2, day3]));
    }

    [Test]
    public void Calculate_ExcludesDates_NotPresentInEveryIndicatorSeries()
    {
        var day1 = new DateTime(2024, 5, 1);
        var day2 = new DateTime(2024, 5, 2); // present everywhere except RSI
        var day3 = new DateTime(2024, 5, 3);
        var aggregator = new SignalAggregator();

        var result = aggregator.Calculate(
            Concat(PriceSeries(day1), PriceSeries(day2), PriceSeries(day3)),
            Concat(MacdSeries(day1), MacdSeries(day2), MacdSeries(day3)),
            Concat(RsiSeries(day1), RsiSeries(day3)), // day2 deliberately missing
            Concat(BollingerSeries(day1), BollingerSeries(day2), BollingerSeries(day3)),
            Concat(AdxSeries(day1), AdxSeries(day2), AdxSeries(day3)),
            Concat(ObvSeries(day1), ObvSeries(day2), ObvSeries(day3)),
            Concat(AtrSeries(day1), AtrSeries(day2), AtrSeries(day3)),
            Concat(CciSeries(day1), CciSeries(day2), CciSeries(day3)),
            Concat(CmfSeries(day1), CmfSeries(day2), CmfSeries(day3)),
            Concat(KeltnerSeries(day1), KeltnerSeries(day2), KeltnerSeries(day3)),
            Concat(SuperTrendSeries(day1), SuperTrendSeries(day2), SuperTrendSeries(day3)),
            Concat(ParabolicSarSeries(day1), ParabolicSarSeries(day2), ParabolicSarSeries(day3)),
            Concat(DonchianSeries(day1), DonchianSeries(day2), DonchianSeries(day3)),
            Concat(MfiSeries(day1), MfiSeries(day2), MfiSeries(day3)),
            Concat(WilliamsPercentRSeries(day1), WilliamsPercentRSeries(day2), WilliamsPercentRSeries(day3)),
            Concat(AroonSeries(day1), AroonSeries(day2), AroonSeries(day3)));

        Assert.That(result.Select(p => p.PriceDate), Is.EqualTo([day1, day3]));
    }

    [Test]
    public void Calculate_PriceSeriesDates_DoNotRestrictTheAlignedDateSet()
    {
        var indicatorDate = new DateTime(2024, 5, 10);
        var extraPriceOnlyDate = new DateTime(2024, 5, 11);
        var aggregator = new SignalAggregator();

        var result = aggregator.Calculate(
            Concat(PriceSeries(indicatorDate), PriceSeries(extraPriceOnlyDate)),
            MacdSeries(indicatorDate),
            RsiSeries(indicatorDate),
            BollingerSeries(indicatorDate),
            AdxSeries(indicatorDate),
            ObvSeries(indicatorDate),
            AtrSeries(indicatorDate),
            CciSeries(indicatorDate),
            CmfSeries(indicatorDate),
            KeltnerSeries(indicatorDate),
            SuperTrendSeries(indicatorDate),
            ParabolicSarSeries(indicatorDate),
            DonchianSeries(indicatorDate),
            MfiSeries(indicatorDate),
            WilliamsPercentRSeries(indicatorDate),
            AroonSeries(indicatorDate));

        Assert.That(result.Select(p => p.PriceDate), Is.EqualTo([indicatorDate]));
    }

    #endregion
}
