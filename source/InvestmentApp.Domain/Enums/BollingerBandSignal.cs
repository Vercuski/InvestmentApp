namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates where a price closed relative to the Bollinger Bands at a given point in the series.
/// </summary>
public enum BollingerBandSignal
{
    WithinBands,
    AboveUpperBand,
    BelowLowerBand
}
