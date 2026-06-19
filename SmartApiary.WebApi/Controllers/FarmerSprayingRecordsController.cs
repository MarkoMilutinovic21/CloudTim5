namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.SprayingRecords.Commands;
using SmartApiary.Application.Features.SprayingRecords.Queries;

[ApiController]
[Route("api/farmer/spraying-records")]
[Authorize(Roles = "Farmer")]
public class FarmerSprayingRecordsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateSprayingRecordCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(new { id = result.Id, windWarning = result.WindWarning });
    }

    [HttpGet("{parcelId}")]
    public async Task<IActionResult> Get(Guid parcelId, CancellationToken ct)
    {
        var records = await mediator.Send(new GetSprayingRecordsQuery(parcelId), ct);
        return Ok(records);
    }
}
