namespace InvestmentApp.Application.Contracts.Poco;

public record TradeSignalPointPoco
{
    public string? TickerSymbol { get; set; }
    public decimal Close { get; set; }
    public string? Action { get; set; }
    public decimal Confidence { get; set; }
    public string? Regime { get; set; }
    public decimal StopLossPrice { get; set; }
    public DateTime Date { get; set; }
    public DateTime PriceDate { get; set; }

    public TradeSignalPointPoco() { }
}
