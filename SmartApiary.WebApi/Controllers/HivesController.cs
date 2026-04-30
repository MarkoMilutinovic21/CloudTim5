namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Hives.Commands;
using SmartApiary.Application.Features.Hives.Queries;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class HivesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{apiaryId}")]
    public async Task<IActionResult> GetHives(Guid apiaryId, CancellationToken ct)
    {
        var hives = await mediator.Send(new GetHivesQuery(apiaryId), ct);
        return Ok(hives);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHive([FromBody] CreateHiveCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Košnica uspešno kreirana." });
    }

    [HttpDelete("{apiaryId}/{hiveId}")]
    public async Task<IActionResult> DeleteHive(Guid apiaryId, Guid hiveId, CancellationToken ct)
    {
        await mediator.Send(new DeleteHiveCommand(hiveId, apiaryId), ct);
        return Ok(new { message = "Košnica uspešno obrisana." });
    }
}