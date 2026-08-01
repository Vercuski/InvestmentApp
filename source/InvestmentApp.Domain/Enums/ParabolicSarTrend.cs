namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates which side of price the Parabolic SAR dot currently sits on. Like
/// <see cref="SuperTrendTrend"/>, Parabolic SAR has no neutral state: the stop is always
/// trailing either below price (an uptrend) or above it (a downtrend).
/// </summary>
public enum ParabolicSarTrend
{
    Bullish,
    Bearish
}
