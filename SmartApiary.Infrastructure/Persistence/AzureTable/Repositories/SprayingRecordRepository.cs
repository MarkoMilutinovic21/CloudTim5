namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class SprayingRecordRepository(IOptions<AzureTableOptions> options) : ISprayingRecordRepository
{
    private readonly TableClient _tableClient = new(options.Value.ConnectionString, "SprayingRecords");

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

    public async Task<IReadOnlyCollection<SprayingRecord>> GetByParcelIdAsync(
        Guid parcelId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        var results = new List<SprayingRecord>();

        await foreach (SprayingRecordEntity entity in _tableClient.QueryAsync<SprayingRecordEntity>(
            x => x.PartitionKey == parcelId.ToString(), cancellationToken: ct))
        {
            results.Add(SprayingRecord.Rehydrate(
                Guid.Parse(entity.RowKey),
                entity.StartTime,
                entity.DurationHours,
                entity.ChemicalName,
                parcelId));
        }

        IEnumerable<SprayingRecord> filtered = results;

        if (from.HasValue)
            filtered = filtered.Where(r => r.StartTime.Date >= from.Value.Date);

        if (to.HasValue)
            filtered = filtered.Where(r => r.StartTime.Date <= to.Value.Date);

        return filtered.OrderByDescending(r => r.StartTime).ToList().AsReadOnly();
    }
}
