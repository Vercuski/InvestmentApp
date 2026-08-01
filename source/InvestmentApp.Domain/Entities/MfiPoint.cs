using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Money Flow Index series: the MFI value and the
/// overbought/oversold/neutral zone it falls into.
/// </summary>
public sealed record MfiPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public MfiZone Zone { get; set; }

    public MfiPoint() { }

    public MfiPoint(string? tickerSymbol, DateTime priceDate, decimal value, MfiZone zone)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
