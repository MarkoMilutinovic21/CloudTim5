namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.PesticideTreatments.Commands;
using SmartApiary.Application.Features.PesticideTreatments.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Farmer")]
public class PesticideTreatmentsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetTreatments(CancellationToken ct)
    {
        IReadOnlyCollection<PesticideTreatmentDto> treatments =
            await mediator.Send(new GetPesticideTreatmentsQuery(GetUserId()), ct);

        return Ok(treatments);
    }

    [HttpGet("notification-status")]
    public async Task<IActionResult> GetNotificationStatus(CancellationToken ct)
    {
        PesticideTreatmentNotificationStatusOverviewDto overview =
            await mediator.Send(new GetPesticideTreatmentNotificationStatusesQuery(GetUserId()), ct);

        return Ok(overview);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTreatment(
        [FromBody] CreatePesticideTreatmentRequest request,
        CancellationToken ct)
    {
        CreatePesticideTreatmentResult result = await mediator.Send(
            new CreatePesticideTreatmentCommand(
                request.ParcelId,
                request.PlannedStartAt,
                request.DurationHours,
                request.PesticideType,
                GetUserId()),
            ct);

        return Ok(new
        {
            id = result.Id,
            notifiedBeekeepersCount = result.NotifiedBeekeepersCount,
            windWarning = result.WindWarning,
            message = $"Najava tretiranja je uspešno kreirana. Broj obaveštenih pčelara: {result.NotifiedBeekeepersCount}."
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTreatment(
        Guid id,
        [FromBody] UpdatePesticideTreatmentRequest request,
        CancellationToken ct)
    {
        UpdatePesticideTreatmentResult result = await mediator.Send(
            new UpdatePesticideTreatmentCommand(
                id,
                request.ParcelId,
                request.PlannedStartAt,
                request.DurationHours,
                request.PesticideType,
                GetUserId()),
            ct);

        return Ok(new
        {
            notifiedBeekeepersCount = result.NotifiedBeekeepersCount,
            windWarning = result.WindWarning,
            message = $"Najava tretiranja je uspešno izmenjena. Broj obaveštenih pčelara: {result.NotifiedBeekeepersCount}."
        });
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTreatment(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelPesticideTreatmentCommand(id, GetUserId()), ct);

        return Ok(new { message = "Najava tretiranja je uspešno otkazana." });
    }
}

public record CreatePesticideTreatmentRequest(
    Guid ParcelId,
    DateTime PlannedStartAt,
    double DurationHours,
    string? PesticideType);

public record UpdatePesticideTreatmentRequest(
    Guid ParcelId,
    DateTime PlannedStartAt,
    double DurationHours,
    string? PesticideType);
