using System.Diagnostics.CodeAnalysis;

namespace InvestmentApp.Domain.Abstractions;

[ExcludeFromCodeCoverage]
public abstract class Entity : IEntity
{
}

public abstract record RecordEntity : IEntity
{
}