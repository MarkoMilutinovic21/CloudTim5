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

public class SprayingRecordRepository(IOptions<AzureTableOptions> options)
    : ISprayingRecordRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "SprayingRecords");

    public async Task SaveAsync(SprayingRecord record, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        var entity = new SprayingRecordEntity
        {
            PartitionKey = record.ParcelId.ToString(),
            RowKey = record.Id.ToString(),
            StartTime = record.StartTime,
            DurationHours = record.DurationHours,
            ChemicalName = record.ChemicalName
        };

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task<IReadOnlyCollection<SprayingRecord>> GetByParcelIdAsync(Guid parcelId, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<SprayingRecord> results = new();

        await foreach (SprayingRecordEntity entity in _tableClient.QueryAsync<SprayingRecordEntity>(
            x => x.PartitionKey == parcelId.ToString(),
            cancellationToken: ct))
        {
            results.Add(SprayingRecord.Rehydrate(
                Guid.Parse(entity.RowKey),
                entity.StartTime,
                entity.DurationHours,
                entity.ChemicalName,
                parcelId));
        }

        return results.AsReadOnly();
    }
}

