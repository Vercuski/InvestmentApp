namespace InvestmentApp.Application.Contracts.Poco;

public record TradeSignalPointPoco
{
    public string? tickerSymbol { get; set; }
    public decimal close { get; set; }
    public string? action { get; set; }
    public decimal confidence { get; set; }
    public string? regime { get; set; }
    public decimal stopLossPrice { get; set; }
    public DateTime date { get; set; }
    public DateTime priceDate { get; set; }

    public TradeSignalPointPoco() { }
}
