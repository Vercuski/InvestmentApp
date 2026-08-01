using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a SuperTrend series: the current SuperTrend line value and
/// which side of price it is trailing on.
/// </summary>
public sealed record SuperTrendPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public SuperTrendTrend Trend { get; set; }

    public SuperTrendPoint() { }

    public SuperTrendPoint(string? tickerSymbol, DateTime priceDate, decimal value, SuperTrendTrend trend)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Trend = trend;
    }
}
