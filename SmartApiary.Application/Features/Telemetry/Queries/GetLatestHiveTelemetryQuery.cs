namespace SmartApiary.Application.Features.Telemetry.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetLatestHiveTelemetryQuery(Guid HiveId, Guid BeekeeperId) : IRequest<TelemetryMeasurementDto?>;

public class GetLatestHiveTelemetryQueryHandler(
    ITelemetryMeasurementRepository measurementRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository)
    : IRequestHandler<GetLatestHiveTelemetryQuery, TelemetryMeasurementDto?>
{
    public async Task<TelemetryMeasurementDto?> Handle(
        GetLatestHiveTelemetryQuery request,
        CancellationToken ct)
    {
        Hive? hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null)
            throw new KeyNotFoundException("Košnica nije pronađena.");

        Apiary? apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        TelemetryMeasurement? measurement =
            await measurementRepository.GetLatestForHiveAsync(request.HiveId, ct);

        return measurement is null
            ? null
            : GetHiveTelemetryQueryHandler.ToDto(measurement);
    }
}
