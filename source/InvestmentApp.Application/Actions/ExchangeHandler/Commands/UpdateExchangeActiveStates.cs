using InvestmentApp.Application.Abstractions;
using InvestmentApp.Domain.Entities;
using System.Net;

namespace InvestmentApp.Application.Actions.ExchangeHandler.Commands;

public sealed record UpdateExchangeActiveStatesRequest(List<ExchangePoint> Exchanges) : IMediatRCommandRequest<HttpStatusCode>;

internal sealed class UpdateExchangeActiveStatesHandler(IExchangeRepository exchangeRepository)
    : IMediatRCommandHandler<UpdateExchangeActiveStatesRequest, HttpStatusCode>
{
    public async Task<HttpStatusCode> Handle(
        UpdateExchangeActiveStatesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Exchanges.Count == 0)
        {
            return HttpStatusCode.NoContent;
        }

        try
        {
            await exchangeRepository.UpdateActiveStatesAsync(request.Exchanges);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.StackTrace}");
            throw;
        }

        return HttpStatusCode.OK;
    }
}
