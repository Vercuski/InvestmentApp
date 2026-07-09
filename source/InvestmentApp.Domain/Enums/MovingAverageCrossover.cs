namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates whether the fast moving average crossed the slow moving average at a given
/// point in the series. A <see cref="Bullish"/> crossover is commonly called a "Golden
/// Cross"; a <see cref="Bearish"/> crossover is commonly called a "Death Cross".
/// </summary>
public enum MovingAverageCrossover
{
    None,
    Bullish,
    Bearish
}
