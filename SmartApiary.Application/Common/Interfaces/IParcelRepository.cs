namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IParcelRepository
{
    Task<Parcel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Parcel>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Parcel>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(Parcel parcel, CancellationToken ct = default);
    Task UpdateAsync(Parcel parcel, CancellationToken ct = default);
    Task DeleteAsync(Parcel parcel, CancellationToken ct = default);
}
