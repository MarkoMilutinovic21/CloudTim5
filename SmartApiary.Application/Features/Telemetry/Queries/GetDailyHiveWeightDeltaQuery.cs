namespace SmartApiary.Application.Features.Telemetry.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetDailyHiveWeightDeltaQuery(
    Guid HiveId,
    Guid BeekeeperId,
    DateTime From,
    DateTime To) : IRequest<IReadOnlyCollection<DailyHiveWeightDeltaDto>>;

public record DailyHiveWeightDeltaDto(
    DateTime Date,
    double? StartWeightKg,
    double? EndWeightKg,
    double? DeltaKg);

public class GetDailyHiveWeightDeltaQueryHandler(
    ITelemetryMeasurementRepository measurementRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository)
    : IRequestHandler<GetDailyHiveWeightDeltaQuery, IReadOnlyCollection<DailyHiveWeightDeltaDto>>
{
    public async Task<IReadOnlyCollection<DailyHiveWeightDeltaDto>> Handle(
        GetDailyHiveWeightDeltaQuery request,
        CancellationToken ct)
    {
        Hive? hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null)
            throw new KeyNotFoundException("Košnica nije pronađena.");

        Apiary? apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        DateTime from = request.From.Date;
        DateTime to = request.To.Date.AddDays(1).AddTicks(-1);

        IReadOnlyCollection<TelemetryMeasurement> measurements =
            await measurementRepository.GetByHiveIdAsync(request.HiveId, from, to, ct);

        return measurements
            .GroupBy(m => m.MeasuredAt.Date)
            .OrderBy(g => g.Key)
            .Select(ToDailyDelta)
            .ToList()
            .AsReadOnly();
    }

    private static DailyHiveWeightDeltaDto ToDailyDelta(
        IGrouping<DateTime, TelemetryMeasurement> dayMeasurements)
    {
        TelemetryMeasurement? start = dayMeasurements
            .OrderBy(m => Math.Abs((m.MeasuredAt.TimeOfDay - TimeSpan.FromHours(8)).Ticks))
            .FirstOrDefault();

        TelemetryMeasurement? end = dayMeasurements
            .OrderBy(m => Math.Abs((m.MeasuredAt.TimeOfDay - TimeSpan.FromHours(20)).Ticks))
            .FirstOrDefault();

        double? delta = start is null || end is null
            ? null
            : Math.Round(end.WeightKg - start.WeightKg, 2);

        return new DailyHiveWeightDeltaDto(
            dayMeasurements.Key,
            start?.WeightKg,
            end?.WeightKg,
            delta);
    }
}
