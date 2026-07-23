using InvestmentApp.Application.Actions.StockDataHandler.Commands;
using InvestmentApp.Application.Actions.StockDataHandler.Queries;
using InvestmentApp.Application.Actions.TickerHandler.Queries;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class StockDataController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [Route("{tickerSymbol}")]
    public async Task<ActionResult<List<StockData>>> GetStockDataByTickerSymbolAsync(string tickerSymbol)
    {
        var result = await mediator.Send(new GetStockDataByTickerSymbolRequest(tickerSymbol));
        var stockDataList = result.ToList();
        if (stockDataList.Count == 0)
        {
            return NotFound($"No stock data found for ticker '{tickerSymbol}'.");
        }
        return Ok(stockDataList);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("{tickerSymbol}/latest")]
    public async Task<ActionResult<StockData>> GetLatestStockDataByTickerSymbolAsync(string tickerSymbol)
    {
        var result = await mediator.Send(new GetLatestStockDataByTickerSymbolRequest(tickerSymbol));
        if (result is null)
        {
            return NotFound($"No stock data found for ticker '{tickerSymbol}'.");
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<List<StockData>>> DownloadSingleStockDataPoint([FromQuery] string TickerSymbol)
    {
        var ticker = await mediator.Send(new GetTickerBySymbolRequest(TickerSymbol));
        var result = await mediator.Send(new DownloadSingleStockDataPointRequest(ticker!));
        if (result is not HttpStatusCode.OK)
        {
            return BadRequest("No Event Found");
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("Download/All")]
    public async Task<IActionResult> DownloadAllStockDataAsync()
    {
        var tickerList = await mediator.Send(new GetTickerListRequest());
        await mediator.Send(new CreateStockDownloadRequest(tickerList));
        return Ok();
    }
}
