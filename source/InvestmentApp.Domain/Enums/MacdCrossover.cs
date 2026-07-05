namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates whether the MACD line crossed the signal line at a given point in the series.
/// </summary>
public enum MacdCrossover
{
    None,
    Bullish,
    Bearish
}