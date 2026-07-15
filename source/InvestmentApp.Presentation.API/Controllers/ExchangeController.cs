using InvestmentApp.Application.Actions.ExchangeHandler.Commands;
using InvestmentApp.Application.Actions.ExchangeHandler.Queries;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ExchangeController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<ExchangePoint>>> GetExchangesAsync()
    {
        var result = await mediator.Send(new GetExchangeListRequest());
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPut]
    [Route("")]
    public async Task<IActionResult> UpdateExchangeActiveStatesAsync([FromBody] List<ExchangePoint> exchanges)
    {
        var result = await mediator.Send(new UpdateExchangeActiveStatesRequest(exchanges));
        return StatusCode((int)result);
    }
}
