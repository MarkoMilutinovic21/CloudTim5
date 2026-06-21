namespace SmartApiary.Application.Features.HiveJournal.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record UpdateJournalEntryCommand(
    Guid EntryId,
    Guid HiveId,
    DateTime EntryDate,
    string Title,
    string Content,
    string BottomBoardColor,
    int HoneyFrames,
    double HoneyKg,
    int BroodFrames,
    bool QueenPresent,
    Guid BeekeeperId) : IRequest;

public class UpdateJournalEntryCommandValidator : AbstractValidator<UpdateJournalEntryCommand>
{
    public UpdateJournalEntryCommandValidator()
    {
        RuleFor(x => x.EntryId).NotEmpty();
        RuleFor(x => x.HiveId).NotEmpty();
        RuleFor(x => x.EntryDate).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.BottomBoardColor).MaximumLength(100);
        RuleFor(x => x.HoneyFrames).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HoneyKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BroodFrames).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BeekeeperId).NotEmpty();
    }
}

public class UpdateJournalEntryCommandHandler(
    IHiveJournalEntryRepository journalRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : IRequestHandler<UpdateJournalEntryCommand>
{
    public async Task Handle(UpdateJournalEntryCommand request, CancellationToken ct)
    {
        var entry = await journalRepository.GetByIdAsync(request.EntryId, ct);

        if (entry is null)
            throw new KeyNotFoundException("Zapis u dnevniku nije pronađen.");

        if (entry.HiveId != request.HiveId)
            throw new UnauthorizedAccessException("Zapis ne pripada ovoj košnici.");

        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        var apiary = hive is null ? null : await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        entry.Update(
            request.EntryDate,
            request.Title,
            request.Content,
            request.BottomBoardColor,
            request.HoneyFrames,
            request.HoneyKg,
            request.BroodFrames,
            request.QueenPresent);

        await journalRepository.UpdateAsync(entry, ct);
    }
}
