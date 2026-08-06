using InvestmentApp.Application.Abstractions;
using InvestmentApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net;

namespace InvestmentApp.Application.Actions.ExchangeHandler.Commands;

public sealed record UpdateExchangeActiveStatesRequest(List<ExchangePoint> Exchanges) : IMediatRCommandRequest<HttpStatusCode>;

internal sealed class UpdateExchangeActiveStatesHandler(IExchangeRepository exchangeRepository,
    ILogger<UpdateExchangeActiveStatesHandler> logger)
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
            logger.LogError("An error occurred: {StackTrace}", ex.StackTrace);
            throw;
        }

        return HttpStatusCode.OK;
    }
}
