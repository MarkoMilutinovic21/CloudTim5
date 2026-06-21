namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Users.Commands;
using SmartApiary.Application.Features.Users.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/beekeeper/settings")]
[Authorize(Roles = "Beekeeper")]
public sealed class BeekeeperSettingsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await mediator.Send(new GetBeekeeperSettingsQuery(GetUserId()), ct));

    [HttpPut]
    public async Task<IActionResult> Update(UpdateBeekeeperSettingsRequest request, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateBeekeeperSettingsCommand(GetUserId(), request.WeightDropThresholdKg), ct);
        return Ok(new { message = "Podešavanja upozorenja su sačuvana." });
    }
}

public record UpdateBeekeeperSettingsRequest(double WeightDropThresholdKg);
