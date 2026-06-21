namespace SmartApiary.Application.Features.Hives.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record UpdateHiveCommand(
    Guid HiveId,
    Guid ApiaryId,
    string Name,
    string HiveType,
    string ExtensionColor,
    int QueenAge,
    string Description,
    Guid OwnerId) : IRequest;

public class UpdateHiveCommandValidator : AbstractValidator<UpdateHiveCommand>
{
    public UpdateHiveCommandValidator()
    {
        RuleFor(x => x.HiveId).NotEmpty();
        RuleFor(x => x.ApiaryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HiveType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ExtensionColor).NotEmpty().MaximumLength(50);
        RuleFor(x => x.QueenAge).GreaterThanOrEqualTo(0).LessThanOrEqualTo(10);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class UpdateHiveCommandHandler(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : IRequestHandler<UpdateHiveCommand>
{
    public async Task Handle(UpdateHiveCommand request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);

        if (apiary is null)
            throw new KeyNotFoundException("Pčelinjak nije pronađen.");

        if (apiary.OwnerId != request.OwnerId)
            throw new UnauthorizedAccessException("Nemate pristup ovom pčelinjaku.");

        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);

        if (hive is null)
            throw new KeyNotFoundException("Košnica nije pronađena.");

        if (hive.ApiaryId != request.ApiaryId)
            throw new UnauthorizedAccessException("Košnica ne pripada ovom pčelinjaku.");

        hive.Update(
            request.Name,
            request.HiveType,
            request.ExtensionColor,
            request.QueenAge,
            request.Description);

        await hiveRepository.UpdateAsync(hive, ct);
    }
}
