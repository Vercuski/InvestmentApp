namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates where a price closed relative to the Keltner Channel at a given point in the series.
/// </summary>
public enum KeltnerChannelSignal
{
    WithinChannel,
    AboveUpperBand,
    BelowLowerBand
}
