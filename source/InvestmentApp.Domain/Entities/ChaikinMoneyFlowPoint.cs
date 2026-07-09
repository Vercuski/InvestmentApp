using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of a Chaikin Money Flow series: the CMF value and whether it
/// reflects meaningful buying pressure, selling pressure, or neither.
/// </summary>
public sealed record ChaikinMoneyFlowPoint : RecordEntity
{
    public int TickerId { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }
    public ChaikinMoneyFlowZone Zone { get; set; }

    public ChaikinMoneyFlowPoint() { }

    public ChaikinMoneyFlowPoint(int tickerId, DateTime priceDate, decimal value, ChaikinMoneyFlowZone zone)
    {
        TickerId = tickerId;
        PriceDate = priceDate;
        Value = value;
        Zone = zone;
    }
}
