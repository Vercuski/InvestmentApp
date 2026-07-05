using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InvestmentApp.Application.Actions.SampleEntity1Dapper.Commands;

public sealed record UpdateSampleEntity1DapperRequest(SampleEntityDefinition SampleEntity) : IMediatRCommandRequest<int>;
internal sealed class UpdateSampleEntity1DapperHandler(IDbConnectionFactory connectionFactory,
    ILogger<UpdateSampleEntity1DapperHandler> logger) : IMediatRCommandHandler<UpdateSampleEntity1DapperRequest, int>
{
    public async Task<int> Handle(
        UpdateSampleEntity1DapperRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = "UPDATE table1 SET value1 = @value1 WHERE value2=@value2";
            using var connection = connectionFactory.CreateWriteConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, request.SampleEntity);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating SampleEntity1Dapper.");
        }
        return 0;
    }
}
