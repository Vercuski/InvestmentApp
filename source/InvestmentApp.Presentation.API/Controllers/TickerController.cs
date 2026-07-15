using InvestmentApp.Application.Actions.TickerHandler.Commands;
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
}
