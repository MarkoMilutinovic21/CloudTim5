namespace SmartApiary.Application.Features.HiveJournal.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteJournalEntryCommand(Guid HiveId, Guid EntryId, Guid BeekeeperId) : IRequest;

public class DeleteJournalEntryCommandHandler(
    IHiveJournalEntryRepository journalRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : IRequestHandler<DeleteJournalEntryCommand>
{
    public async Task Handle(DeleteJournalEntryCommand request, CancellationToken ct)
    {
        var entry = await journalRepository.GetByIdAsync(request.EntryId, ct);
        if (entry is null) throw new KeyNotFoundException("Zapis u dnevniku nije pronađen.");
        if (entry.HiveId != request.HiveId) throw new UnauthorizedAccessException("Zapis ne pripada ovoj košnici.");

        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        var apiary = hive is null ? null : await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        await journalRepository.DeleteAsync(entry, ct);
    }
}
