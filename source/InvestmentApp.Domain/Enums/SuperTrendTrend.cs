namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates which side of price the SuperTrend line currently sits on. Unlike a
/// crossover-based enum such as <see cref="MacdCrossover"/>, SuperTrend has no neutral
/// state &#8212; the line is always trailing either below price (an uptrend) or above it
/// (a downtrend), so every bar in the series carries a definite value.
/// </summary>
public enum SuperTrendTrend
{
    Bullish,
    Bearish
}
