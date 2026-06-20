namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Devices.Commands;
using SmartApiary.Application.Features.Hives.Commands;
using SmartApiary.Application.Features.Hives.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class HivesController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{apiaryId}")]
    public async Task<IActionResult> GetHives(Guid apiaryId, CancellationToken ct)
    {
        var hives = await mediator.Send(new GetHivesQuery(apiaryId, GetUserId()), ct);
        return Ok(hives);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHive([FromBody] CreateHiveCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { OwnerId = GetUserId() }, ct);
        return Ok(new { message = "Košnica uspešno kreirana." });
    }

    [HttpPut("{apiaryId}/{hiveId}")]
    public async Task<IActionResult> UpdateHive(
        Guid apiaryId,
        Guid hiveId,
        [FromBody] UpdateHiveRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new UpdateHiveCommand(
            hiveId,
            apiaryId,
            request.Name,
            request.HiveType,
            request.ExtensionColor,
            request.QueenAge,
            request.Description ?? string.Empty,
            GetUserId()), ct);

        return Ok(new { message = "Košnica uspešno izmenjena." });
    }

    [HttpDelete("{apiaryId}/{hiveId}")]
    public async Task<IActionResult> DeleteHive(Guid apiaryId, Guid hiveId, CancellationToken ct)
    {
        await mediator.Send(new DeleteHiveCommand(hiveId, apiaryId, GetUserId()), ct);
        return Ok(new { message = "Košnica uspešno obrisana." });
    }

    [HttpPost("{apiaryId}/{hiveId}/devices")]
    public async Task<IActionResult> RegisterDevice(
        Guid apiaryId,
        Guid hiveId,
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterDeviceForHiveCommand(
            apiaryId,
            hiveId,
            request.SerialNumber,
            GetUserId()), ct);

        return Ok(result);
    }
}

public class UpdateHiveRequest
{
    public string Name { get; set; } = string.Empty;
    public string HiveType { get; set; } = string.Empty;
    public string ExtensionColor { get; set; } = string.Empty;
    public int QueenAge { get; set; }
    public string? Description { get; set; }
}

public class RegisterDeviceRequest
{
    public string SerialNumber { get; set; } = string.Empty;
}
