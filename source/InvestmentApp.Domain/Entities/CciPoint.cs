using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a CCI series: the CCI value and the
/// overbought/oversold/neutral zone it falls into.
/// </summary>
public sealed record CciPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public CciZone Zone { get; set; }

    public CciPoint() { }

    public CciPoint(string? tickerSymbol, DateTime priceDate, decimal value, CciZone zone)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
