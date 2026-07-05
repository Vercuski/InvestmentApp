using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a MACD series: the MACD line, the signal line,
/// their difference (the histogram), and any crossover that occurred at this point.
/// </summary>
public sealed record MacdPoint : RecordEntity
{
    public DateOnly PriceDate { get; }
    public decimal Macd { get; }
    public decimal Signal { get; }
    public decimal Histogram { get; }
    public MacdCrossover Crossover { get; }

    public MacdPoint(DateOnly priceDate, decimal macd, decimal signal, decimal histogram, MacdCrossover crossover)
    {
        PriceDate = priceDate;
        Macd = macd;
        Signal = signal;
        Histogram = histogram;
        Crossover = crossover;
    }
}