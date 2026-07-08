using InvestmentApp.Domain.Entities;
using InvestmentApp.Application.Actions.StockDataHandler.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvestmentApp.Application.Actions.StockDataHandler.Commands;
using InvestmentApp.Application.Actions.TickerHandler.Queries;

namespace InvestmentApp.Presentation.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CalculationController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<List<StockData>>> RunAllCalculations()
    {
        var result = await mediator.Send(new RunCalculationRequest());

        return Ok(result);
    }
}
