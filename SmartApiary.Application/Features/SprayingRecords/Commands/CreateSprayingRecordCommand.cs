namespace SmartApiary.Application.Features.SprayingRecords.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateSprayingRecordCommand(
    DateTime StartTime,
    double DurationHours,
    string ChemicalName,
    Guid ParcelId
) : IRequest<Guid>;

public class CreateSprayingRecordValidator : AbstractValidator<CreateSprayingRecordCommand>
{
    public CreateSprayingRecordValidator()
    {
        RuleFor(x => x.ParcelId)
            .NotEmpty();

        RuleFor(x => x.DurationHours)
            .GreaterThan(0);

        RuleFor(x => x.ChemicalName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.StartTime)
            .NotEmpty();
    }
}

public class CreateSprayingRecordHandler(
    ISprayingRecordRepository repo
) : IRequestHandler<CreateSprayingRecordCommand, Guid>
{
    public async Task<Guid> Handle(CreateSprayingRecordCommand request, CancellationToken ct)
    {
        var record = SprayingRecord.Create(
            request.StartTime,
            request.DurationHours,
            request.ChemicalName,
            request.ParcelId
        );

        await repo.SaveAsync(record, ct);

        return record.Id;
    }
}

