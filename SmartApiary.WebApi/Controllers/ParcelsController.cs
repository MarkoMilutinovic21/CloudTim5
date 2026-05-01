namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Parcels.Commands;
using SmartApiary.Application.Features.Parcels.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Farmer")]
public class ParcelsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetParcels(CancellationToken ct)
    {
        var parcels = await mediator.Send(new GetParcelsQuery(GetUserId()), ct);

        return Ok(parcels);
    }

    [HttpPost]
    public async Task<IActionResult> CreateParcel([FromBody] CreateParcelRequest request, CancellationToken ct)
    {
        Guid parcelId = await mediator.Send(new CreateParcelCommand(
            request.Name,
            request.Area,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.Description,
            GetUserId()), ct);

        return Ok(new
        {
            id = parcelId,
            message = "Parcela uspešno kreirana."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateParcel(
        Guid id,
        [FromBody] UpdateParcelRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new UpdateParcelCommand(
            id,
            request.Name,
            request.Area,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.Description,
            GetUserId()), ct);

        return Ok(new { message = "Parcela uspešno izmenjena." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteParcel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteParcelCommand(id, GetUserId()), ct);

        return Ok(new { message = "Parcela uspešno obrisana." });
    }
}

public record CreateParcelRequest(
    string Name,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string Description);

public record UpdateParcelRequest(
    string Name,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string Description);