using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Abstractions;
using InvestmentApp.Infrastructure.HealthChecks;
using InvestmentApp.Persistence.ConnectionFactory;
using System.Reflection;

namespace InvestmentApp.Tests.ArchitectureTests;

internal static class AssemblyReferences
{
    internal static readonly Assembly DomainAssembly = typeof(RecordEntity).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(IDbConnectionFactory).Assembly;
    internal static readonly Assembly InfrastrcutureAssembly = typeof(SimpleHealthCheck).Assembly;
    internal static readonly Assembly PersistenceAssembly = typeof(SqlDbConnectionFactory).Assembly;
    internal static readonly Assembly TestsAssembly = typeof(DomainArchitectureTests).Assembly;
}
