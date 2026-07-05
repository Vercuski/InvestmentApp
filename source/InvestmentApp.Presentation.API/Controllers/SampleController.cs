using InvestmentApp.Application.Actions.SampleEntity1Dapper.Queries;
using InvestmentApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentApp.Presentation.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SampleController(IMediator mediator) : ControllerBase
{
    // GET api/<SampleController>/5
    [HttpGet("Dapper/{id}")]
    public async Task<SampleEntityDefinition?> GetDapper(int id)
    {
        GetSingleSampleEntity1DapperRequest request = new(id);
        var returnValue = await mediator.Send(request, CancellationToken.None);
        return returnValue;
    }
}
