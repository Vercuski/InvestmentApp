namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates whether ADX signals a trend strong enough to trust directional signals from
/// other indicators, or a weak/ranging market where such signals are less reliable.
/// </summary>
public enum AdxTrendStrength
{
    Weak,
    Strong
}
