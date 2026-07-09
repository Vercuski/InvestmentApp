using InvestmentApp.Application.Actions.StockDataHandler.Commands;
using InvestmentApp.Application.Actions.StockDataHandler.Queries;
using InvestmentApp.Application.Actions.TickerHandler.Queries;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class StockDataController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<List<StockData>>> GetStockDataAsync([FromBody] Ticker ticker)
    {
        var result = await mediator.Send(new GetStockRequest(ticker));
        if (result is null)
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
