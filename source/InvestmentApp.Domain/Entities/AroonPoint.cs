using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an Aroon series: Aroon Up, Aroon Down, the Aroon
/// Oscillator (their difference), and the trend they describe together.
/// </summary>
public sealed record AroonPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal AroonUp { get; set; }
    public decimal AroonDown { get; set; }
    public decimal Oscillator { get; set; }
    public AroonTrend Trend { get; set; }

    public AroonPoint() { }

    public AroonPoint(
        string? tickerSymbol,
        DateTime priceDate,
        decimal aroonUp,
        decimal aroonDown,
        decimal oscillator,
        AroonTrend trend)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        AroonUp = aroonUp;
        AroonDown = aroonDown;
        Oscillator = oscillator;
        Trend = trend;
    }
}
