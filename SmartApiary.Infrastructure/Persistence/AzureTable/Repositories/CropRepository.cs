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

public class CropRepository(IOptions<AzureTableOptions> options) : ICropRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "Crops");

    public async Task SaveAsync(Crop crop, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        var entity = MapToEntity(crop);

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task<IReadOnlyCollection<Crop>> GetByParcelIdAsync(Guid parcelId, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<Crop> results = new();

        await foreach (CropEntity entity in _tableClient.QueryAsync<CropEntity>(
            x => x.PartitionKey == parcelId.ToString(),
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    private static Crop MapToDomain(CropEntity entity)
    {
        return Crop.Rehydrate(
            Guid.Parse(entity.RowKey),
            entity.Name,
            entity.SowingDate,
            Guid.Parse(entity.PartitionKey));
    }

    private static CropEntity MapToEntity(Crop crop)
    {
        return new CropEntity
        {
            PartitionKey = crop.ParcelId.ToString(),
            RowKey = crop.Id.ToString(),
            Name = crop.Name,
            SowingDate = crop.SowingDate
        };
    }
}
