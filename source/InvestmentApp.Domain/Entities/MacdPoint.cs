using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a MACD series: the MACD line, the signal line,
/// their difference (the histogram), and any crossover that occurred at this point.
/// </summary>
public sealed record MacdPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Macd { get; set; }
    public decimal Signal { get; set; }
    public decimal Histogram { get; set; }
    public MacdCrossover Crossover { get; set; }

    public MacdPoint() { }
    public MacdPoint(string? tickerSymbol, DateTime priceDate, decimal macd, decimal signal, decimal histogram, MacdCrossover crossover)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Macd = macd;
        Signal = signal;
        Histogram = histogram;
        Crossover = crossover;
    }
}