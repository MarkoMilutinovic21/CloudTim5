namespace SmartApiary.Application.Features.HiveJournal.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetJournalEntriesQuery(Guid HiveId, Guid BeekeeperId) : IRequest<IReadOnlyCollection<JournalEntryDto>>;

public record JournalEntryDto(
    Guid Id,
    Guid HiveId,
    DateTime EntryDate,
    string Title,
    string Content,
    string BottomBoardColor,
    int HoneyFrames,
    double HoneyKg,
    int BroodFrames,
    bool QueenPresent,
    DateTime CreatedAt);

public class GetJournalEntriesQueryHandler(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository,
    IHiveJournalEntryRepository journalRepository) : IRequestHandler<GetJournalEntriesQuery, IReadOnlyCollection<JournalEntryDto>>
{
    public async Task<IReadOnlyCollection<JournalEntryDto>> Handle(GetJournalEntriesQuery request, CancellationToken ct)
    {
        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null) throw new KeyNotFoundException("Košnica nije pronađena.");

        var apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        var entries = await journalRepository.GetByHiveIdAsync(request.HiveId, ct);

        return entries
            .OrderByDescending(entry => entry.EntryDate)
            .Select(entry => new JournalEntryDto(
                entry.Id,
                entry.HiveId,
                entry.EntryDate,
                entry.Title,
                entry.Content,
                entry.BottomBoardColor,
                entry.HoneyFrames,
                entry.HoneyKg,
                entry.BroodFrames,
                entry.QueenPresent,
                entry.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
