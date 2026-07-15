using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Stochastic Oscillator series: %K and %D, the zone %D
/// falls into, and any %K/%D crossover that occurred at this point.
/// </summary>
public sealed record StochasticPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal PercentK { get; set; }
    public decimal PercentD { get; set; }
    public StochasticZone Zone { get; set; }
    public StochasticCrossover Crossover { get; set; }

    public StochasticPoint() { }

    public StochasticPoint(
        string? tickerSymbol,
        DateTime priceDate,
        decimal percentK,
        decimal percentD,
        StochasticZone zone,
        StochasticCrossover crossover)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        PercentK = percentK;
        PercentD = percentD;
        Zone = zone;
        Crossover = crossover;
    }
}
