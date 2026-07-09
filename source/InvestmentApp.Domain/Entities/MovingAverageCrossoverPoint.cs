using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a moving average crossover series: the fast and slow
/// averages, their difference, and any crossover that occurred at this point.
/// </summary>
public sealed record MovingAverageCrossoverPoint : RecordEntity
{
    public int TickerId { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal FastAverage { get; set; }
    public decimal SlowAverage { get; set; }
    public decimal Difference { get; set; }
    public MovingAverageCrossover Crossover { get; set; }

    public MovingAverageCrossoverPoint() { }

    public MovingAverageCrossoverPoint(
        int tickerId,
        DateTime priceDate,
        decimal fastAverage,
        decimal slowAverage,
        decimal difference,
        MovingAverageCrossover crossover)
    {
        TickerId = tickerId;
        PriceDate = priceDate;
        FastAverage = fastAverage;
        SlowAverage = slowAverage;
        Difference = difference;
        Crossover = crossover;
    }
}
