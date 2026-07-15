using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single computed point of an Average True Range series: the smoothed volatility
/// magnitude at that bar.
/// </summary>
/// <remarks>
/// Unlike the other indicator points in this namespace, <see cref="AtrPoint"/> has no
/// categorical zone/signal field. ATR measures the magnitude of price movement, not
/// direction or overbought/oversold state, so there is no natural threshold to classify
/// it against; it is typically consumed directly for position sizing or stop-loss distance.
/// </remarks>
public sealed record AtrPoint : RecordEntity
{
    public string? TickerSymbol { get; set; }
    public DateTime PriceDate { get; set; }
    public decimal Value { get; set; }

    public AtrPoint() { }

    public AtrPoint(string? tickerSymbol, DateTime priceDate, decimal value)
    {
        TickerSymbol = tickerSymbol;
        PriceDate = priceDate;
        Value = value;
    }
}
