using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an RSI series: the RSI value (0-100) and the
/// overbought/oversold/neutral zone it falls into.
/// </summary>
public sealed record RsiPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public RsiZone Zone { get; set; }

    public RsiPoint() { }

    public RsiPoint(string? tickerSymbol, DateTime priceDate, decimal value, RsiZone zone)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
