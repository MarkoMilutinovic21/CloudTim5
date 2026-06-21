namespace SmartApiary.Application.Features.Telemetry.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetHiveTelemetryQuery(
    Guid HiveId,
    Guid BeekeeperId,
    DateTime? From = null,
    DateTime? To = null) : IRequest<IReadOnlyCollection<TelemetryMeasurementDto>>;

public record TelemetryMeasurementDto(
    Guid Id,
    Guid DeviceId,
    Guid HiveId,
    Guid DeviceUuid,
    double WeightKg,
    double TemperatureC,
    double HumidityPercent,
    double BatteryPercent,
    DateTime MeasuredAt,
    DateTime ReceivedAt);

public class GetHiveTelemetryQueryHandler(
    ITelemetryMeasurementRepository measurementRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository)
    : IRequestHandler<GetHiveTelemetryQuery, IReadOnlyCollection<TelemetryMeasurementDto>>
{
    public async Task<IReadOnlyCollection<TelemetryMeasurementDto>> Handle(
        GetHiveTelemetryQuery request,
        CancellationToken ct)
    {
        Hive? hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null)
            throw new KeyNotFoundException("Košnica nije pronađena.");

        Apiary? apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        DateTime? to = request.To;

        if (to.HasValue && to.Value.TimeOfDay == TimeSpan.Zero)
            to = to.Value.Date.AddDays(1).AddTicks(-1);

        IReadOnlyCollection<TelemetryMeasurement> measurements =
            await measurementRepository.GetByHiveIdAsync(
                request.HiveId,
                request.From,
                to,
                ct);

        return measurements
            .Select(ToDto)
            .ToList()
            .AsReadOnly();
    }

    internal static TelemetryMeasurementDto ToDto(TelemetryMeasurement measurement)
    {
        return new TelemetryMeasurementDto(
            measurement.Id,
            measurement.DeviceId,
            measurement.HiveId,
            measurement.DeviceUuid,
            measurement.WeightKg,
            measurement.TemperatureC,
            measurement.HumidityPercent,
            measurement.BatteryPercent,
            measurement.MeasuredAt,
            measurement.ReceivedAt);
    }
}
