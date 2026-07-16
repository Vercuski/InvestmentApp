using InvestmentApp.Application.Actions.PositionHandler.Commands;
using InvestmentApp.Application.Actions.PositionHandler.Queries;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PositionsController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<PositionPoint>>> GetPositionsAsync()
    {
        var result = await mediator.Send(new GetPositionListRequest());
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPut]
    [Route("")]
    public async Task<ActionResult<PositionPoint>> UpsertPositionAsync([FromBody] PositionPoint position)
    {
        var result = await mediator.Send(new UpsertPositionRequest(position));
        return Ok(result);
    }
}
