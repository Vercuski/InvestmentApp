using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Parabolic SAR series: the current stop level and which
/// side of price it is trailing on.
/// </summary>
public sealed record ParabolicSarPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public ParabolicSarTrend Trend { get; set; }

    public ParabolicSarPoint() { }

    public ParabolicSarPoint(string? tickerSymbol, DateTime priceDate, decimal value, ParabolicSarTrend trend)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Trend = trend;
    }
}
