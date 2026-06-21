namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class TelemetryMeasurementRepository(IOptions<AzureTableOptions> options)
    : ITelemetryMeasurementRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        options.Value.MeasurementsTable);

    public async Task SaveAsync(TelemetryMeasurement measurement, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        await _tableClient.UpsertEntityAsync(MapToEntity(measurement), TableUpdateMode.Replace, ct);
    }

    public async Task<TelemetryMeasurement?> GetLatestForHiveAsync(Guid hiveId, CancellationToken ct = default)
    {
        IReadOnlyCollection<TelemetryMeasurement> measurements =
            await GetByHiveIdAsync(hiveId, ct: ct);

        return measurements
            .OrderByDescending(m => m.MeasuredAt)
            .FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<TelemetryMeasurement>> GetByHiveIdAsync(
        Guid hiveId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<TelemetryMeasurement> results = new();

        await foreach (TelemetryMeasurementEntity entity in _tableClient.QueryAsync<TelemetryMeasurementEntity>(
            e => e.PartitionKey == hiveId.ToString(),
            cancellationToken: ct))
        {
            TelemetryMeasurement measurement = MapToDomain(entity);

            if (from.HasValue && measurement.MeasuredAt < from.Value)
                continue;

            if (to.HasValue && measurement.MeasuredAt > to.Value)
                continue;

            results.Add(measurement);
        }

        return results
            .GroupBy(m => new { m.DeviceUuid, m.MeasuredAt })
            .Select(group => group.OrderByDescending(m => m.ReceivedAt).First())
            .OrderBy(m => m.MeasuredAt)
            .ToList()
            .AsReadOnly();
    }

    private static TelemetryMeasurement MapToDomain(TelemetryMeasurementEntity entity)
    {
        Guid deviceUuid = Guid.Parse(entity.DeviceUuid);
        Guid storedId = Guid.Parse(entity.RowKey.Split('|')[1]);
        Guid measurementId = storedId == deviceUuid
            ? CreateStableMeasurementId(deviceUuid, entity.MeasuredAt)
            : storedId;

        return TelemetryMeasurement.Load(
            measurementId,
            Guid.Parse(entity.DeviceId),
            Guid.Parse(entity.HiveId),
            deviceUuid,
            entity.WeightKg,
            entity.TemperatureC,
            entity.HumidityPercent,
            entity.BatteryPercent,
            entity.MeasuredAt,
            entity.ReceivedAt);
    }

    private static TelemetryMeasurementEntity MapToEntity(TelemetryMeasurement measurement)
    {
        return new TelemetryMeasurementEntity
        {
            PartitionKey = measurement.HiveId.ToString(),
            RowKey = $"{measurement.MeasuredAt.Ticks:D19}|{CreateStableMeasurementId(measurement.DeviceUuid, measurement.MeasuredAt)}",
            DeviceId = measurement.DeviceId.ToString(),
            HiveId = measurement.HiveId.ToString(),
            DeviceUuid = measurement.DeviceUuid.ToString(),
            WeightKg = measurement.WeightKg,
            TemperatureC = measurement.TemperatureC,
            HumidityPercent = measurement.HumidityPercent,
            BatteryPercent = measurement.BatteryPercent,
            MeasuredAt = measurement.MeasuredAt,
            ReceivedAt = measurement.ReceivedAt
        };
    }

    private static Guid CreateStableMeasurementId(Guid deviceUuid, DateTime measuredAt)
    {
        byte[] idBytes = deviceUuid.ToByteArray();
        byte[] ticksBytes = BitConverter.GetBytes(measuredAt.ToUniversalTime().Ticks);

        for (int i = 0; i < ticksBytes.Length; i++)
            idBytes[i] ^= ticksBytes[i];

        return new Guid(idBytes);
    }
}
