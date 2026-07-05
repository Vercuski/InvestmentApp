using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Options;

public sealed record LogOptions : IBaseOptionsConfig
{
    public LogLevel LogLevel { get; set; } = null!;
    public string Section => "Logging";
}

public sealed record LogLevel : IBaseOptionsConfig
{
    public string Default { get; set; } = null!;
    public string MicrosoftAspNetCore { get; set; } = null!;
    public string Section => "Logging:LogLevel";
}
