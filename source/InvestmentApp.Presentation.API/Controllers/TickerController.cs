using InvestmentApp.Application.Actions.TickerHandler.Commands;
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
public class TickerController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("Download")]
    public async Task<ActionResult<HttpStatusCode>> DownloadTickerSymbolsAsync()
    {
        var result = await mediator.Send(new DownloadTickerSymbolsRequest());
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("{tickerSymbol}")]
    public async Task<ActionResult<Ticker>> GetTickerBySymbolAsync(string tickerSymbol)
    {
        var result = await mediator.Send(new GetTickerBySymbolRequest(tickerSymbol));
        if (result is null)
        {
            return NotFound($"No ticker found for symbol '{tickerSymbol}'.");
        }
        return Ok(result);
    }
}
