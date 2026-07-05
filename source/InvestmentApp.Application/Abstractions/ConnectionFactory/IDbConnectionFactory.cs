using System.Data;

namespace InvestmentApp.Application.Abstractions.ConnectionFactory;

public interface IDbConnectionFactory
{
    IDbConnection CreateReadConnection();
    IDbConnection CreateWriteConnection();
}
