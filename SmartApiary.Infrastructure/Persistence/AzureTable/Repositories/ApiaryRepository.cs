using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class ApiaryRepository(IOptions<AzureTableOptions> options) : IApiaryRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString, "Apiaries");

    public async Task<Apiary?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ownerId = id.ToString();
        await foreach (var entity in _tableClient.QueryAsync<ApiaryEntity>(
            e => e.RowKey == id.ToString(), cancellationToken: ct))
        {
            return MapToDomain(entity);
        }
        return null;
    }

    public async Task<IReadOnlyCollection<Apiary>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var results = new List<Apiary>();
        await foreach (var entity in _tableClient.QueryAsync<ApiaryEntity>(
            e => e.PartitionKey == ownerId.ToString(), cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }
        return results.AsReadOnly();
    }

    public async Task SaveAsync(Apiary apiary, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        var entity = MapToEntity(apiary);
        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateAsync(Apiary apiary, CancellationToken ct = default)
    {
        var entity = MapToEntity(apiary);
        await _tableClient.UpdateEntityAsync(entity, Azure.ETag.All, TableUpdateMode.Replace, ct);
    }

    public async Task DeleteAsync(Apiary apiary, CancellationToken ct = default)
    {
        await _tableClient.DeleteEntityAsync(
            apiary.OwnerId.ToString(), apiary.Id.ToString(), Azure.ETag.All, ct);
    }

    private static Apiary MapToDomain(ApiaryEntity entity)
    {
        return Apiary.Create(
            entity.Name,
            entity.Description,
            entity.Location,
            entity.Latitude,
            entity.Longitude,
            Guid.Parse(entity.OwnerId));
    }

    private static ApiaryEntity MapToEntity(Apiary apiary)
    {
        return new ApiaryEntity
        {
            PartitionKey = apiary.OwnerId.ToString(),
            RowKey = apiary.Id.ToString(),
            Name = apiary.Name,
            Description = apiary.Description,
            Location = apiary.Location,
            Latitude = apiary.Latitude,
            Longitude = apiary.Longitude,
            OwnerId = apiary.OwnerId.ToString(),
            CreatedAt = apiary.CreatedAt
        };
    }
}