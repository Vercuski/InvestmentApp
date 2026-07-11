using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single composed point of a strategy series: the recommended <see cref="TradeAction"/>,
/// the market regime that produced it, a confidence score, and an ATR-derived stop-loss
/// price for risk sizing. Produced by <see cref="InvestmentApp.Application.Calculators.SignalAggregator"/>
/// from the already-computed outputs of individual indicator calculators.
/// </summary>
public sealed record TradeSignalPoint : RecordEntity
{
    public int TickerId { get; set; }
    public DateTime PriceDate { get; set; }
    public TradeAction Action { get; set; }
    public MarketRegime Regime { get; set; }

    /// <summary>
    /// A 0-1 score reflecting how strongly the contributing signals agree. 0 for
    /// <see cref="TradeAction.Hold"/>; higher when confirming signals (e.g. OBV trend
    /// direction) line up with the primary signal, lower when they don't.
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>The ATR value at this point, carried through for position-sizing use downstream.</summary>
    public decimal AtrValue { get; set; }

    /// <summary>
    /// ATR-derived stop-loss price for this signal. Null when <see cref="Action"/> is
    /// <see cref="TradeAction.Hold"/>, since no position is being suggested.
    /// </summary>
    public decimal? StopLossPrice { get; set; }

    public TradeSignalPoint() { }

    public TradeSignalPoint(
        int tickerId,
        DateTime priceDate,
        TradeAction action,
        MarketRegime regime,
        decimal confidence,
        decimal atrValue,
        decimal? stopLossPrice)
    {
        TickerId = tickerId;
        PriceDate = priceDate;
        Action = action;
        Regime = regime;
        Confidence = confidence;
        AtrValue = atrValue;
        StopLossPrice = stopLossPrice;
    }
}
