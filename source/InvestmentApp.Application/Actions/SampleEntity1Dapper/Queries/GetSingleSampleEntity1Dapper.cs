using Dapper;
using InvestmentApp.Application.Abstractions;
using InvestmentApp.Application.Abstractions.ConnectionFactory;
using InvestmentApp.Domain.Entities;

namespace InvestmentApp.Application.Actions.SampleEntity1Dapper.Queries;

public sealed record GetSingleSampleEntity1DapperRequest(int Id) : IMediatRQueryRequest<SampleEntityDefinition?>;
internal sealed class GetSingleSampleEntity1DapperHandler(
    IDbConnectionFactory connectionFactory
    ) : IMediatRQueryHandler<GetSingleSampleEntity1DapperRequest, SampleEntityDefinition?>
{
    public async Task<SampleEntityDefinition?> Handle(
        GetSingleSampleEntity1DapperRequest request,
        CancellationToken cancellationToken)
    {
        var sql = "SELECT * FROM table1 WHERE value1 = @value1";
        using var connection = connectionFactory.CreateReadConnection();
        var response = await connection.QuerySingleAsync<SampleEntityDefinition>(sql, request.Id);

        if (response is null)
        {
            return new SampleEntityDefinition();
        }

        return response;
    }
}
