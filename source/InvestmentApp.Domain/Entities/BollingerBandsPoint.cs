using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Bollinger Bands series: the price used for the
/// calculation, the middle/upper/lower bands, and where the price sits relative to them.
/// </summary>
public sealed record BollingerBandsPoint : RecordEntity
{
    public int TickerId { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Price { get; set; }
    public decimal MiddleBand { get; set; }
    public decimal UpperBand { get; set; }
    public decimal LowerBand { get; set; }
    public BollingerBandSignal Signal { get; set; }

    public BollingerBandsPoint() { }

    public BollingerBandsPoint(
        int tickerId,
        DateTime priceDate,
        decimal price,
        decimal middleBand,
        decimal upperBand,
        decimal lowerBand,
        BollingerBandSignal signal)
    {
        TickerId = tickerId;
        PriceDate = priceDate;
        Price = price;
        MiddleBand = middleBand;
        UpperBand = upperBand;
        LowerBand = lowerBand;
        Signal = signal;
    }
}
