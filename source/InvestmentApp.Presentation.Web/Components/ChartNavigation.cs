namespace InvestmentApp.Presentation.Web.Components;

/// <summary>
/// Builds links to the Chart page for a given ticker symbol. Shared by any page that
/// lists ticker symbols (Signals, Positions, etc.) so clicking a ticker jumps straight
/// to its chart, pre-populated and auto-generated.
/// </summary>
public static class ChartNavigation
{
    public static string BuildChartUrl(string? tickerSymbol)
    {
        return string.IsNullOrWhiteSpace(tickerSymbol)
            ? "/chart"
            : $"/chart/{Uri.EscapeDataString(tickerSymbol)}";
    }
}
