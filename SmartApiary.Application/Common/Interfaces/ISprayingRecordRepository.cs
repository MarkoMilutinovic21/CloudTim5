namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface ISprayingRecordRepository
{
    Task SaveAsync(SprayingRecord record, CancellationToken ct = default);
    Task<SprayingRecord?> GetByTreatmentIdAsync(Guid treatmentId, CancellationToken ct = default);
    Task<IReadOnlyCollection<SprayingRecord>> GetByParcelIdAsync(
        Guid parcelId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
}
