namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Telemetry.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/hives/{hiveId:guid}/telemetry")]
[Authorize(Roles = "Beekeeper")]
public class TelemetryController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(Guid hiveId, CancellationToken ct)
    {
        TelemetryMeasurementDto? measurement =
            await mediator.Send(new GetLatestHiveTelemetryQuery(hiveId, GetUserId()), ct);

        return measurement is null
            ? NotFound(new { message = "Nema telemetrijskih merenja za ovu košnicu." })
            : Ok(measurement);
    }

    [HttpGet("measurements")]
    public async Task<IActionResult> GetMeasurements(
        Guid hiveId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        IReadOnlyCollection<TelemetryMeasurementDto> measurements =
            await mediator.Send(new GetHiveTelemetryQuery(hiveId, GetUserId(), from, to), ct);

        return Ok(measurements);
    }

    [HttpGet("daily-weight-delta")]
    public async Task<IActionResult> GetDailyWeightDelta(
        Guid hiveId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        DateTime toDate = to?.Date ?? DateTime.UtcNow.Date;
        DateTime fromDate = from?.Date ?? toDate.AddDays(-7);

        if (fromDate > toDate)
            return BadRequest(new { message = "Parametar 'from' mora biti pre parametra 'to'." });

        IReadOnlyCollection<DailyHiveWeightDeltaDto> deltas =
            await mediator.Send(new GetDailyHiveWeightDeltaQuery(hiveId, GetUserId(), fromDate, toDate), ct);

        return Ok(deltas);
    }
}
