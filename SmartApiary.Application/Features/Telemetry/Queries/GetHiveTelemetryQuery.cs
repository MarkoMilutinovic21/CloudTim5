namespace SmartApiary.Application.Features.Telemetry.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetHiveTelemetryQuery(
    Guid HiveId,
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
    ITelemetryMeasurementRepository measurementRepository)
    : IRequestHandler<GetHiveTelemetryQuery, IReadOnlyCollection<TelemetryMeasurementDto>>
{
    public async Task<IReadOnlyCollection<TelemetryMeasurementDto>> Handle(
        GetHiveTelemetryQuery request,
        CancellationToken ct)
    {
        IReadOnlyCollection<TelemetryMeasurement> measurements =
            await measurementRepository.GetByHiveIdAsync(
                request.HiveId,
                request.From,
                request.To,
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
