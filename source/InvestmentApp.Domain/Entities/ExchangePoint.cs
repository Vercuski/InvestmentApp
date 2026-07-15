using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an ADX series: the trend-strength value itself, the
/// +DI/-DI directional components it was derived from, and the trend-strength zone.
/// </summary>
public sealed record ExchangePoint : RecordEntity
{
    public string? ExchangeSymbol { get; set; }
    public string? ExchangeDescription { get; set; }
    public bool Active { get; set; }

    public ExchangePoint() { }

    public ExchangePoint(string? exchangeSymbol, string? exchangeDescription, bool active)
    {
        ExchangeSymbol = exchangeSymbol;
        ExchangeDescription = exchangeDescription;
        Active = active;
    }
}
