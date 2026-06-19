namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class HiveJournalEntryRepository(IOptions<AzureTableOptions> options) : IHiveJournalEntryRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "HiveJournalEntries");

    public async Task<HiveJournalEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (HiveJournalEntryEntity entity in _tableClient.QueryAsync<HiveJournalEntryEntity>(
            entry => entry.RowKey == id.ToString(),
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task<IReadOnlyCollection<HiveJournalEntry>> GetByHiveIdAsync(Guid hiveId, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<HiveJournalEntry> results = new();

        await foreach (HiveJournalEntryEntity entity in _tableClient.QueryAsync<HiveJournalEntryEntity>(
            entry => entry.PartitionKey == hiveId.ToString(),
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    public async Task SaveAsync(HiveJournalEntry entry, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        HiveJournalEntryEntity entity = MapToEntity(entry);

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateAsync(HiveJournalEntry entry, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        HiveJournalEntryEntity entity = MapToEntity(entry);

        await _tableClient.UpdateEntityAsync(
            entity,
            ETag.All,
            TableUpdateMode.Replace,
            ct);
    }

    public async Task DeleteAsync(HiveJournalEntry entry, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await _tableClient.DeleteEntityAsync(
            entry.HiveId.ToString(),
            entry.Id.ToString(),
            ETag.All,
            ct);
    }

    private static HiveJournalEntry MapToDomain(HiveJournalEntryEntity entity)
    {
        return HiveJournalEntry.Rehydrate(
            Guid.Parse(entity.RowKey),
            Guid.Parse(entity.HiveId),
            entity.EntryDate,
            entity.Title,
            entity.Content,
            entity.BottomBoardColor,
            entity.HoneyFrames,
            entity.HoneyKg,
            entity.BroodFrames,
            entity.QueenPresent,
            entity.CreatedAt);
    }

    private static HiveJournalEntryEntity MapToEntity(HiveJournalEntry entry)
    {
        return new HiveJournalEntryEntity
        {
            PartitionKey = entry.HiveId.ToString(),
            RowKey = entry.Id.ToString(),
            HiveId = entry.HiveId.ToString(),
            EntryDate = entry.EntryDate,
            Title = entry.Title,
            Content = entry.Content,
            BottomBoardColor = entry.BottomBoardColor,
            HoneyFrames = entry.HoneyFrames,
            HoneyKg = entry.HoneyKg,
            BroodFrames = entry.BroodFrames,
            QueenPresent = entry.QueenPresent,
            CreatedAt = entry.CreatedAt
        };
    }
}
