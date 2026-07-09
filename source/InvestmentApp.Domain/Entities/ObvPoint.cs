using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an On-Balance Volume series: the cumulative OBV value,
/// its moving-average signal line, and any crossover that occurred at this point.
/// </summary>
public sealed record ObvPoint : RecordEntity
{
    public int TickerId { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public decimal SignalLine { get; set; }
    public ObvTrend Trend { get; set; }

    public ObvPoint() { }

    public ObvPoint(int tickerId, DateTime priceDate, decimal value, decimal signalLine, ObvTrend trend)
    {
        TickerId = tickerId;
        PriceDate = priceDate;
        Value = value;
        SignalLine = signalLine;
        Trend = trend;
    }
}
