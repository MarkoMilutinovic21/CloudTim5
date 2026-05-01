namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class ParcelRepository(IOptions<AzureTableOptions> options) : IParcelRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "Parcels");

    public async Task<Parcel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (ParcelEntity entity in _tableClient.QueryAsync<ParcelEntity>(
            parcel => parcel.RowKey == id.ToString(),
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task<IReadOnlyCollection<Parcel>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<Parcel> results = new();

        await foreach (ParcelEntity entity in _tableClient.QueryAsync<ParcelEntity>(
            parcel => parcel.PartitionKey == ownerId.ToString(),
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    public async Task SaveAsync(Parcel parcel, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        ParcelEntity entity = MapToEntity(parcel);

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateAsync(Parcel parcel, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        ParcelEntity entity = MapToEntity(parcel);

        await _tableClient.UpdateEntityAsync(
            entity,
            ETag.All,
            TableUpdateMode.Replace,
            ct);
    }

    public async Task DeleteAsync(Parcel parcel, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await _tableClient.DeleteEntityAsync(
            parcel.OwnerId.ToString(),
            parcel.Id.ToString(),
            ETag.All,
            ct);
    }

    private static Parcel MapToDomain(ParcelEntity entity)
    {
        return Parcel.Rehydrate(
            Guid.Parse(entity.RowKey),
            entity.Name,
            entity.Area,
            entity.Location,
            entity.Latitude,
            entity.Longitude,
            entity.Description,
            Guid.Parse(entity.OwnerId),
            entity.CreatedAt);
    }

    private static ParcelEntity MapToEntity(Parcel parcel)
    {
        return new ParcelEntity
        {
            PartitionKey = parcel.OwnerId.ToString(),
            RowKey = parcel.Id.ToString(),
            Name = parcel.Name,
            Area = parcel.Area,
            Location = parcel.Location,
            Latitude = parcel.Latitude,
            Longitude = parcel.Longitude,
            Description = parcel.Description,
            OwnerId = parcel.OwnerId.ToString(),
            CreatedAt = parcel.CreatedAt
        };
    }
}