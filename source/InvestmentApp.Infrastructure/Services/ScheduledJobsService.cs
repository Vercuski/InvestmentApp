using InvestmentApp.Application.Actions.CalculationHandler.Commands;
using InvestmentApp.Application.Actions.StockDataHandler.Commands;
using InvestmentApp.Application.Actions.StockDataHandler.Queries;
using InvestmentApp.Application.Actions.TickerHandler.Queries;
using InvestmentApp.Application.Services;
using MediatR;

namespace InvestmentApp.Infrastructure.Services;

public sealed class ScheduledJobsService(IMediator mediator) : IScheduledJobsService
{
    public async Task RunDailyDownloadAndCalculationAsync()
    {
        var tickerList = await mediator.Send(new GetTickerListRequest());
        await mediator.Send(new CreateStockDownloadRequest(tickerList));
        await mediator.Send(new RunCalculationRequest());
    }

    public async Task DownloadOpenPositionStockDataAsync()
    {
        var tickerList = await mediator.Send(new GetOpenPositionTickersRequest());
        await mediator.Send(new CreateStockDownloadForTickersRequest(tickerList));
    }
}