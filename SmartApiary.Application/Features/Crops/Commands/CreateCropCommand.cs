namespace SmartApiary.Application.Features.Crops.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateCropCommand(
    string Name,
    DateTime SowingDate,
    Guid ParcelId,
    Guid FarmerId
) : IRequest<Guid>;

public class CreateCropCommandValidator : AbstractValidator<CreateCropCommand>
{
    public CreateCropCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ParcelId)
            .NotEmpty();

        RuleFor(x => x.FarmerId).NotEmpty();

        RuleFor(x => x.SowingDate)
            .LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public class CreateCropCommandHandler(
    ICropRepository cropRepository,
    IParcelRepository parcelRepository
) : IRequestHandler<CreateCropCommand, Guid>
{
    public async Task<Guid> Handle(CreateCropCommand request, CancellationToken ct)
    {
        Parcel? parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is null)
            throw new KeyNotFoundException("Parcela nije pronađena.");
        if (parcel.OwnerId != request.FarmerId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");

        var crop = Crop.Create(
            request.Name,
            request.SowingDate,
            request.ParcelId
        );

        await cropRepository.SaveAsync(crop, ct);

        return crop.Id;
    }
}
