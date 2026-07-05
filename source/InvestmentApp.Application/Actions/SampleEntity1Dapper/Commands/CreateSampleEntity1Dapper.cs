using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InvestmentApp.Application.Actions.SampleEntity1Dapper.Commands;

public sealed record CreateSampleEntity1DapperRequest(SampleEntityDefinition SampleEntity) : IMediatRCommandRequest<int>;
internal sealed class CreateSampleEntity1DapperHandler(IDbConnectionFactory connectionFactory,
    ILogger<CreateSampleEntity1DapperHandler> logger)
    : IMediatRCommandHandler<CreateSampleEntity1DapperRequest, int>
{
    public async Task<int> Handle(
        CreateSampleEntity1DapperRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = "INSERT INTO table1 (value1, value2) VALUES (@value1, @value2)";
            using var connection = connectionFactory.CreateWriteConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, request.SampleEntity);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating SampleEntity1Dapper.");
        }
        return 0;
    }
}
