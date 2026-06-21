namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class PesticideTreatmentRepository(IOptions<AzureTableOptions> options)
    : IPesticideTreatmentRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "PesticideTreatments");

    public async Task<PesticideTreatment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (PesticideTreatmentEntity entity in _tableClient.QueryAsync<PesticideTreatmentEntity>(
            treatment => treatment.RowKey == id.ToString(),
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task<IReadOnlyCollection<PesticideTreatment>> GetByFarmerIdAsync(
        Guid farmerId,
        CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<PesticideTreatment> results = new();

        await foreach (PesticideTreatmentEntity entity in _tableClient.QueryAsync<PesticideTreatmentEntity>(
            treatment => treatment.PartitionKey == farmerId.ToString(),
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<PesticideTreatment>> GetAllAsync(CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        List<PesticideTreatment> results = new();
        await foreach (PesticideTreatmentEntity entity in _tableClient.QueryAsync<PesticideTreatmentEntity>(
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    public async Task SaveAsync(PesticideTreatment treatment, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        PesticideTreatmentEntity entity = MapToEntity(treatment);

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateAsync(PesticideTreatment treatment, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        PesticideTreatmentEntity entity = MapToEntity(treatment);

        await _tableClient.UpdateEntityAsync(
            entity,
            ETag.All,
            TableUpdateMode.Replace,
            ct);
    }

    private static PesticideTreatment MapToDomain(PesticideTreatmentEntity entity)
    {
        return PesticideTreatment.Rehydrate(
            Guid.Parse(entity.RowKey),
            Guid.Parse(entity.ParcelId),
            Guid.Parse(entity.FarmerId),
            EnsureUtc(entity.PlannedStartAt),
            entity.DurationHours,
            entity.PesticideType,
            entity.Status,
            entity.NotifiedBeekeepersCount,
            EnsureUtc(entity.CreatedAt),
            EnsureNullableUtc(entity.UpdatedAt),
            EnsureNullableUtc(entity.CancelledAt),
            EnsureNullableUtc(entity.WeatherObservedAt),
            entity.WeatherDescription,
            entity.WindSpeedMs,
            entity.HadPrecipitation);
    }

    private static PesticideTreatmentEntity MapToEntity(PesticideTreatment treatment)
    {
        return new PesticideTreatmentEntity
        {
            PartitionKey = treatment.FarmerId.ToString(),
            RowKey = treatment.Id.ToString(),
            ParcelId = treatment.ParcelId.ToString(),
            FarmerId = treatment.FarmerId.ToString(),

            PlannedStartAt = EnsureUtc(treatment.PlannedStartAt),
            DurationHours = treatment.DurationHours,
            PesticideType = treatment.PesticideType,
            Status = treatment.Status,
            NotifiedBeekeepersCount = treatment.NotifiedBeekeepersCount,

            CreatedAt = EnsureUtc(treatment.CreatedAt),
            UpdatedAt = EnsureNullableUtc(treatment.UpdatedAt),
            CancelledAt = EnsureNullableUtc(treatment.CancelledAt),
            WeatherObservedAt = EnsureNullableUtc(treatment.WeatherObservedAt),
            WeatherDescription = treatment.WeatherDescription,
            WindSpeedMs = treatment.WindSpeedMs,
            HadPrecipitation = treatment.HadPrecipitation
        };
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        if (value.Kind == DateTimeKind.Local)
        {
            return value.ToUniversalTime();
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static DateTime? EnsureNullableUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return EnsureUtc(value.Value);
    }
}
