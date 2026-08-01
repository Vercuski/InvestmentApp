using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Donchian Channels series: the closing price used for
/// reference, the upper/middle/lower bands, and whether this bar's high or low made a new
/// N-period extreme.
/// </summary>
public sealed record DonchianChannelsPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Price { get; set; }
    public decimal UpperBand { get; set; }
    public decimal MiddleLine { get; set; }
    public decimal LowerBand { get; set; }
    public DonchianChannelSignal Signal { get; set; }

    public DonchianChannelsPoint() { }

    public DonchianChannelsPoint(
        string? tickerSymbol,
        DateTime priceDate,
        decimal price,
        decimal upperBand,
        decimal middleLine,
        decimal lowerBand,
        DonchianChannelSignal signal)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Price = price;
        UpperBand = upperBand;
        MiddleLine = middleLine;
        LowerBand = lowerBand;
        Signal = signal;
    }
}
