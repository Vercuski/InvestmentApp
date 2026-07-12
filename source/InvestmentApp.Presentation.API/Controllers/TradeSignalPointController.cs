using InvestmentApp.Application.Actions.StockDataHandler.Commands;
using InvestmentApp.Application.Actions.StockDataHandler.Queries;
using InvestmentApp.Application.Actions.TickerHandler.Queries;
using InvestmentApp.Application.Contracts.Poco;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TradeSignalPointController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [Route("LatestBuys")]
    public async Task<ActionResult<List<TradeSignalPointPoco>>> GetLatestBuyTradeSignalPointsRequest()
    {
        var result = await mediator.Send(new GetLatestBuyTradeSignalPointsRequest());
        if (result is null)
        {
            return BadRequest("No Buy Events Found");
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("LatestSells")]
    public async Task<ActionResult<List<TradeSignalPointPoco>>> GetLatestSellTradeSignalPointsRequest()
    {
        var result = await mediator.Send(new GetLatestSellTradeSignalPointsRequest());
        if (result is null)
        {
            return BadRequest("No Sell Events Found");
        }
        return Ok(result);
    }

}
