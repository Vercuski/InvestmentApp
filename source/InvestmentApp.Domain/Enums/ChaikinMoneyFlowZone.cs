namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates whether Chaikin Money Flow shows meaningful buying pressure, selling
/// pressure, or is too close to zero to call either way.
/// </summary>
public enum ChaikinMoneyFlowZone
{
    Neutral,
    Bullish,
    Bearish
}
