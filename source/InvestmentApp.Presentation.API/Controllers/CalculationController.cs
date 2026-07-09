using InvestmentApp.Application.Actions.CalculationHandler.Commands;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
