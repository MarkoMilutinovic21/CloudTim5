namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Apiaries.Commands;
using SmartApiary.Application.Features.Apiaries.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class ApiariesController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetApiaries(CancellationToken ct)
    {
        var apiaries = await mediator.Send(new GetApiaresQuery(GetUserId()), ct);
        return Ok(apiaries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApiary([FromBody] CreateApiaryCommand command, CancellationToken ct)
    {
        var commandWithOwner = command with { OwnerId = GetUserId() };
        await mediator.Send(commandWithOwner, ct);
        return Ok(new { message = "Pčelinjak uspešno kreiran." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiary(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteApiaryCommand(id, GetUserId()), ct);
        return Ok(new { message = "Pčelinjak uspešno obrisan." });
    }
}