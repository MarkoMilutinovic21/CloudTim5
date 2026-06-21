namespace SmartApiary.Application.Features.HiveJournal.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateJournalEntryCommand(
    Guid HiveId,
    DateTime EntryDate,
    string Title,
    string Content,
    string BottomBoardColor,
    int HoneyFrames,
    double HoneyKg,
    int BroodFrames,
    bool QueenPresent,
    Guid BeekeeperId) : IRequest<Guid>;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
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

public class CreateJournalEntryCommandHandler(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository,
    IHiveJournalEntryRepository journalRepository) : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken ct)
    {
        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null) throw new KeyNotFoundException("Košnica nije pronađena.");

        var apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);
        if (apiary is null || apiary.OwnerId != request.BeekeeperId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj košnici.");

        var entry = HiveJournalEntry.Create(
            request.HiveId,
            request.EntryDate,
            request.Title,
            request.Content,
            request.BottomBoardColor,
            request.HoneyFrames,
            request.HoneyKg,
            request.BroodFrames,
            request.QueenPresent);

        await journalRepository.SaveAsync(entry, ct);

        return entry.Id;
    }
}
