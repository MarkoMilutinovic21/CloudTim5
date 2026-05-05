namespace SmartApiary.Application.Features.HiveJournal.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteJournalEntryCommand(Guid HiveId, Guid EntryId) : IRequest;

public class DeleteJournalEntryCommandHandler(
    IHiveJournalEntryRepository journalRepository) : IRequestHandler<DeleteJournalEntryCommand>
{
    public async Task Handle(DeleteJournalEntryCommand request, CancellationToken ct)
    {
        var entry = await journalRepository.GetByIdAsync(request.EntryId, ct);
        if (entry is null) throw new Exception("Zapis u dnevniku nije pronađen.");
        if (entry.HiveId != request.HiveId) throw new Exception("Zapis ne pripada ovoj košnici.");

        await journalRepository.DeleteAsync(entry, ct);
    }
}
