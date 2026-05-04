namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface ITelemetryMeasurementRepository
{
    Task SaveAsync(TelemetryMeasurement measurement, CancellationToken ct = default);
    Task<TelemetryMeasurement?> GetLatestForHiveAsync(Guid hiveId, CancellationToken ct = default);
    Task<IReadOnlyCollection<TelemetryMeasurement>> GetByHiveIdAsync(
        Guid hiveId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
}
