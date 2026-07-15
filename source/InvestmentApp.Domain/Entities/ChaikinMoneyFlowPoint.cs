using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Chaikin Money Flow series: the CMF value and whether it
/// reflects meaningful buying pressure, selling pressure, or neither.
/// </summary>
public sealed record ChaikinMoneyFlowPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public ChaikinMoneyFlowZone Zone { get; set; }

    public ChaikinMoneyFlowPoint() { }

    public ChaikinMoneyFlowPoint(string? tickerSymbol, DateTime priceDate, decimal value, ChaikinMoneyFlowZone zone)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
