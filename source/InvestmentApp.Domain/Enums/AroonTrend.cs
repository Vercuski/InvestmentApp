namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates whether Aroon Up/Down describe a strong uptrend, a strong downtrend, or
/// neither (a consolidating or ambiguous market) at a given point in the series.
/// </summary>
public enum AroonTrend
{
    Neutral,
    Uptrend,
    Downtrend
}
