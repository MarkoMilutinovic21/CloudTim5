namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IHiveJournalEntryRepository
{
    Task<HiveJournalEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<HiveJournalEntry>> GetByHiveIdAsync(Guid hiveId, CancellationToken ct = default);
    Task SaveAsync(HiveJournalEntry entry, CancellationToken ct = default);
    Task UpdateAsync(HiveJournalEntry entry, CancellationToken ct = default);
    Task DeleteAsync(HiveJournalEntry entry, CancellationToken ct = default);
}
