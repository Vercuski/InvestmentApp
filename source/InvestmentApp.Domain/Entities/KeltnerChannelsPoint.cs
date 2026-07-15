using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Keltner Channel series: the price used for the
/// calculation, the middle/upper/lower bands, and where the price sits relative to them.
/// </summary>
public sealed record KeltnerChannelsPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Price { get; set; }
    public decimal MiddleLine { get; set; }
    public decimal UpperBand { get; set; }
    public decimal LowerBand { get; set; }
    public KeltnerChannelSignal Signal { get; set; }

    public KeltnerChannelsPoint() { }

    public KeltnerChannelsPoint(
        string? tickerSymbol,
        DateTime priceDate,
        decimal price,
        decimal middleLine,
        decimal upperBand,
        decimal lowerBand,
        KeltnerChannelSignal signal)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Price = price;
        MiddleLine = middleLine;
        UpperBand = upperBand;
        LowerBand = lowerBand;
        Signal = signal;
    }
}
