namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IPesticideTreatmentRepository
{
    Task<PesticideTreatment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<PesticideTreatment>> GetByFarmerIdAsync(Guid farmerId, CancellationToken ct = default);
    Task<IReadOnlyCollection<PesticideTreatment>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(PesticideTreatment treatment, CancellationToken ct = default);
    Task UpdateAsync(PesticideTreatment treatment, CancellationToken ct = default);
}
