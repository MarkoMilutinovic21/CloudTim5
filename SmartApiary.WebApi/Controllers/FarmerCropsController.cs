namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Crops.Commands;
using SmartApiary.Application.Features.Crops.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/farmer/crops")]
[Authorize(Roles = "Farmer")]
public class FarmerCropsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(CreateCropCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command with { FarmerId = GetUserId() }, ct);
        return Ok(id);
    }

    [HttpGet("{parcelId}")]
    public async Task<IActionResult> Get(Guid parcelId, CancellationToken ct)
    {
        var crops = await mediator.Send(new GetCropsQuery(parcelId, GetUserId()), ct);
        return Ok(crops);
    }
}
