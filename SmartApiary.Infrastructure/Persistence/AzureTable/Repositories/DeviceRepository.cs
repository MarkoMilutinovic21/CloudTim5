namespace SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;

using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class DeviceRepository(IOptions<AzureTableOptions> options) : IDeviceRepository
{
    private readonly TableClient _tableClient = new(
        options.Value.ConnectionString,
        options.Value.DevicesTable);

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (DeviceEntity entity in _tableClient.QueryAsync<DeviceEntity>(
            e => e.RowKey == id.ToString(),
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task<Device?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (DeviceEntity entity in _tableClient.QueryAsync<DeviceEntity>(
            e => e.PartitionKey == serialNumber,
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task<Device?> GetByTokenAsync(string deviceToken, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);

        await foreach (DeviceEntity entity in _tableClient.QueryAsync<DeviceEntity>(
            e => e.DeviceToken == deviceToken,
            cancellationToken: ct))
        {
            return MapToDomain(entity);
        }

        return null;
    }

    public async Task SaveAsync(Device device, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        await _tableClient.AddEntityAsync(MapToEntity(device), ct);
    }

    public async Task UpdateAsync(Device device, CancellationToken ct = default)
    {
        await _tableClient.CreateIfNotExistsAsync(ct);
        await _tableClient.UpdateEntityAsync(MapToEntity(device), ETag.All, TableUpdateMode.Replace, ct);
    }

    private static Device MapToDomain(DeviceEntity entity)
    {
        return Device.Load(
            Guid.Parse(entity.RowKey),
            entity.SerialNumber,
            Guid.Parse(entity.HiveId),
            string.IsNullOrWhiteSpace(entity.DeviceUuid) ? null : Guid.Parse(entity.DeviceUuid),
            entity.DeviceToken,
            entity.Status,
            entity.RegisteredAt,
            entity.PairedAt);
    }

    private static DeviceEntity MapToEntity(Device device)
    {
        return new DeviceEntity
        {
            PartitionKey = device.SerialNumber,
            RowKey = device.Id.ToString(),
            SerialNumber = device.SerialNumber,
            HiveId = device.HiveId.ToString(),
            DeviceUuid = device.DeviceUuid?.ToString(),
            DeviceToken = device.DeviceToken,
            Status = device.Status,
            RegisteredAt = device.RegisteredAt,
            PairedAt = device.PairedAt
        };
    }
}
