using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Entities;

public sealed record Ticker : RecordEntity
{
    public int TickerId { get; set; }

    public string? TickerSymbol { get; set; }

    public string? Description { get; set; }
}