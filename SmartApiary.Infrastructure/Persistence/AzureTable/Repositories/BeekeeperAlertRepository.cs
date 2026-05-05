namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class BeekeeperAlertRepository(IOptions<AzureTableOptions> options)
    : IBeekeeperAlertRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        "BeekeeperAlerts");

    public async Task<IReadOnlyCollection<BeekeeperAlert>> GetByBeekeeperIdAsync(
        Guid beekeeperId,
        CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        List<BeekeeperAlert> results = new();

        await foreach (BeekeeperAlertEntity entity in _tableClient.QueryAsync<BeekeeperAlertEntity>(
            alert => alert.PartitionKey == beekeeperId.ToString(),
            cancellationToken: ct))
        {
            results.Add(MapToDomain(entity));
        }

        return results.AsReadOnly();
    }

    public async Task SaveAsync(BeekeeperAlert alert, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        BeekeeperAlertEntity entity = MapToEntity(alert);

        await _tableClient.AddEntityAsync(entity, ct);
    }

    private static BeekeeperAlert MapToDomain(BeekeeperAlertEntity entity)
    {
        return BeekeeperAlert.Rehydrate(
            Guid.Parse(entity.RowKey),
            Guid.Parse(entity.BeekeeperId),
            entity.Type,
            entity.Title,
            entity.Message,
            entity.CreatedAt);
    }

    private static BeekeeperAlertEntity MapToEntity(BeekeeperAlert alert)
    {
        return new BeekeeperAlertEntity
        {
            PartitionKey = alert.BeekeeperId.ToString(),
            RowKey = alert.Id.ToString(),
            BeekeeperId = alert.BeekeeperId.ToString(),
            Type = alert.Type,
            Title = alert.Title,
            Message = alert.Message,
            CreatedAt = alert.CreatedAt
        };
    }
}
