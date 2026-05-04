namespace SmartApiary.Application.Features.Telemetry.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetLatestHiveTelemetryQuery(Guid HiveId) : IRequest<TelemetryMeasurementDto?>;

public class GetLatestHiveTelemetryQueryHandler(
    ITelemetryMeasurementRepository measurementRepository)
    : IRequestHandler<GetLatestHiveTelemetryQuery, TelemetryMeasurementDto?>
{
    public async Task<TelemetryMeasurementDto?> Handle(
        GetLatestHiveTelemetryQuery request,
        CancellationToken ct)
    {
        TelemetryMeasurement? measurement =
            await measurementRepository.GetLatestForHiveAsync(request.HiveId, ct);

        return measurement is null
            ? null
            : GetHiveTelemetryQueryHandler.ToDto(measurement);
    }
}
