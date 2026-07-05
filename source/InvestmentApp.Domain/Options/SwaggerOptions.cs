using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Domain.Options;

public sealed record SwaggerOptions : IBaseOptionsConfig
{
    public string ServerUrl { get; set; } = null!;
    public string Section => "Swagger";
}
