using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Options;

public sealed record EodDataOptions : IBaseOptionsConfig
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Section => "EodData";
}
