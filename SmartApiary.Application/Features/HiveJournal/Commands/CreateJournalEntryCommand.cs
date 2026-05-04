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
    bool QueenPresent) : IRequest<Guid>;

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
    }
}

public class CreateJournalEntryCommandHandler(
    IHiveRepository hiveRepository,
    IDeviceRepository deviceRepository,
    IHiveJournalEntryRepository journalRepository) : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken ct)
    {
        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        var device = hive is null
            ? await deviceRepository.GetByHiveIdAsync(request.HiveId, ct)
            : null;

        if (hive is null && device is null) throw new Exception("Kosnica nije pronadjena.");

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
