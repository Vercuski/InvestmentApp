using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Entities;

public sealed record Ticker : RecordEntity
{
    public string? TickerSymbol { get; set; }

    public string? Description { get; set; }

    public string? ExchangeSymbol { get; set; }
}