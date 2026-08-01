using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Williams %R series: the %R value and the
/// overbought/oversold/neutral zone it falls into.
/// </summary>
public sealed record WilliamsPercentRPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public WilliamsPercentRZone Zone { get; set; }

    public WilliamsPercentRPoint() { }

    public WilliamsPercentRPoint(string? tickerSymbol, DateTime priceDate, decimal value, WilliamsPercentRZone zone)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
