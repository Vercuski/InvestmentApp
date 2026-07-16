using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Domain.Enums;

namespace InvestmentApp.Domain.Entities;

/// <summary>
/// A single tracked position: the ticker and exchange it was opened on, the market
/// regime and confidence score of the signal that prompted it, and the purchase and
/// (once realized) sale details.
/// </summary>
public sealed record PositionPoint : RecordEntity
{
    /// <summary>Identity primary key. 0 for a position that has not yet been saved.</summary>
    public int PositionId { get; set; }
    public string? TickerSymbol { get; set; }
    public string? ExchangeSymbol { get; set; }
    public MarketRegime Regime { get; set; }

    /// <summary>A 0-1 score reflecting the confidence of the signal that prompted this position.</summary>
    public decimal Confidence { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal NumberOfShares { get; set; }
    public DateTime PurchaseDate { get; set; }

    /// <summary>Null while the position remains open.</summary>
    public decimal? SellPrice { get; set; }

    /// <summary>Null while the position remains open.</summary>
    public DateTime? SellDate { get; set; }

    public PositionPoint() { }

    public PositionPoint(
        int positionId,
        string? tickerSymbol,
        string? exchangeSymbol,
        MarketRegime regime,
        decimal confidence,
        decimal purchasePrice,
        decimal numberOfShares,
        DateTime purchaseDate,
        decimal? sellPrice,
        DateTime? sellDate)
    {
        PositionId = positionId;
        TickerSymbol = tickerSymbol;
        ExchangeSymbol = exchangeSymbol;
        Regime = regime;
        Confidence = confidence;
        PurchasePrice = purchasePrice;
        NumberOfShares = numberOfShares;
        PurchaseDate = purchaseDate;
        SellPrice = sellPrice;
        SellDate = sellDate;
    }
}
