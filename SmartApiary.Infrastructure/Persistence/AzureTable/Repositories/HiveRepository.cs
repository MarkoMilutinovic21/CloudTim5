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

public class HiveRepository(IOptions<AzureTableOptions> options) : IHiveRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString, "Hives");

    public async Task<Hive?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await foreach (var entity in _tableClient.QueryAsync<HiveEntity>(
            e => e.RowKey == id.ToString(), cancellationToken: ct))
        {
            return MapToDomain(entity);
        }
        return null;
    }

    public async Task<IReadOnlyCollection<Hive>> GetByApiaryIdAsync(Guid apiaryId, CancellationToken ct = default)
    {
        var results = new List<Hive>();
        await foreach (var entity in _tableClient.QueryAsync<HiveEntity>(
            e => e.PartitionKey == apiaryId.ToString(), cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }
        return results.AsReadOnly();
    }

    public async Task SaveAsync(Hive hive, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        var entity = MapToEntity(hive);
        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateAsync(Hive hive, CancellationToken ct = default)
    {
        var entity = MapToEntity(hive);
        await _tableClient.UpdateEntityAsync(entity, Azure.ETag.All, TableUpdateMode.Replace, ct);
    }

    public async Task DeleteAsync(Hive hive, CancellationToken ct = default)
    {
        await _tableClient.DeleteEntityAsync(
            hive.ApiaryId.ToString(), hive.Id.ToString(), Azure.ETag.All, ct);
    }

    private static Hive MapToDomain(HiveEntity entity)
    {
        return Hive.Create(
            entity.Name,
            entity.Description,
            Guid.Parse(entity.ApiaryId));
    }

    private static HiveEntity MapToEntity(Hive hive)
    {
        return new HiveEntity
        {
            PartitionKey = hive.ApiaryId.ToString(),
            RowKey = hive.Id.ToString(),
            Name = hive.Name,
            Description = hive.Description,
            ApiaryId = hive.ApiaryId.ToString(),
            CreatedAt = hive.CreatedAt
        };
    }
}