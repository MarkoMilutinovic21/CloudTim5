namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Device?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct = default);
    Task<Device?> GetByTokenAsync(string deviceToken, CancellationToken ct = default);
    Task SaveAsync(Device device, CancellationToken ct = default);
    Task UpdateAsync(Device device, CancellationToken ct = default);
}
