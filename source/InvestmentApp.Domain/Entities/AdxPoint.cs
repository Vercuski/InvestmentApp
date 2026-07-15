using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an ADX series: the trend-strength value itself, the
/// +DI/-DI directional components it was derived from, and the trend-strength zone.
/// </summary>
public sealed record AdxPoint : RecordEntity
{
    public string? TickerSymbol{ get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Adx { get; set; }
    public decimal PlusDi { get; set; }
    public decimal MinusDi { get; set; }
    public AdxTrendStrength TrendStrength { get; set; }

    public AdxPoint() { }

    public AdxPoint(string? tickerSymbol, DateTime priceDate, decimal adx, decimal plusDi, decimal minusDi, AdxTrendStrength trendStrength)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Adx = adx;
        PlusDi = plusDi;
        MinusDi = minusDi;
        TrendStrength = trendStrength;
    }
}
