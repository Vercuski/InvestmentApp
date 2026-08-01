namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates where a bar's high/low sits relative to its own Donchian Channel. Because
/// the channel's bands are the highest high and lowest low of the same trailing window
/// that includes the current bar, <see cref="AboveUpperBand"/> and <see cref="BelowLowerBand"/>
/// naturally identify a new N-period high or low &#8212; a breakout &#8212; rather than a
/// simple over/under-band read the way <see cref="KeltnerChannelSignal"/> does.
/// </summary>
public enum DonchianChannelSignal
{
    WithinChannel,
    AboveUpperBand,
    BelowLowerBand
}
