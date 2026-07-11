namespace InvestmentApp.Domain.Enums;

/// <summary>
/// The actionable recommendation produced by the strategy layer at a given point
/// in the series, after composing one or more indicator signals.
/// </summary>
public enum TradeAction
{
    Hold,
    Buy,
    Sell
}
