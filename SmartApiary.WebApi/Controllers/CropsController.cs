namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Parcels.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class CropsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyCrops([FromQuery] double? radiusKm, CancellationToken ct)
    {
        var crops = await mediator.Send(new GetNearbyCropsQuery(GetUserId(), radiusKm), ct);
        return Ok(crops);
    }
}
