using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace InvestmentApp.Persistence.ConnectionFactory;

public class SqlDbConnectionFactory(IOptions<ConnectionStringOptions> connectionStringOptions) : IDbConnectionFactory
{
    private readonly string _readConnectionString = connectionStringOptions.Value.QueryDbConnection;
    private readonly string _writeConnectionString = connectionStringOptions.Value.CommandDbConnection;

    public IDbConnection CreateReadConnection()
    {
        return new SqlConnection(_readConnectionString);
    }

    public IDbConnection CreateWriteConnection()
    {
        return new SqlConnection(_writeConnectionString);
    }
}
