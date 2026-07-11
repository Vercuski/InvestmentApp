namespace InvestmentApp.Domain.Enums;

/// <summary>
/// Indicates which market state <see cref="Application.Strategies.SignalAggregator"/>
/// detected via ADX at a given point, and therefore which sub-strategy's rules were
/// applied to produce the <see cref="TradeAction"/>.
/// </summary>
public enum MarketRegime
{
    /// <summary>ADX below its trend threshold: mean-reversion rules apply (Bollinger Bands + RSI).</summary>
    Ranging,

    /// <summary>ADX at or above its trend threshold: trend-following rules apply (MACD + OBV).</summary>
    Trending
}
