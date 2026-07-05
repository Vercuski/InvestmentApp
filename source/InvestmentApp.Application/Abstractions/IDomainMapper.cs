using InvestmentApp.Domain.Abstractions;

namespace InvestmentApp.Application.Abstractions;

public interface IDomainMapper<out TEntity>
    where TEntity : Entity
{
    TEntity MapToDomain();
}
