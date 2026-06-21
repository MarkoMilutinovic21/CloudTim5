namespace SmartApiary.Application.Features.Parcels.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record SetParcelCropCommand(
    Guid ParcelId,
    string CropName,
    DateTime FloweringStart,
    DateTime FloweringEnd,
    string CropNotes,
    Guid OwnerId) : IRequest;

public class SetParcelCropCommandValidator : AbstractValidator<SetParcelCropCommand>
{
    public SetParcelCropCommandValidator()
    {
        RuleFor(x => x.ParcelId).NotEmpty();
        RuleFor(x => x.CropName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FloweringStart).NotEmpty();
        RuleFor(x => x.FloweringEnd)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.FloweringStart);
        RuleFor(x => x.CropNotes).MaximumLength(500);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class SetParcelCropCommandHandler(
    IParcelRepository parcelRepository) : IRequestHandler<SetParcelCropCommand>
{
    public async Task Handle(SetParcelCropCommand request, CancellationToken ct)
    {
        var parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is null) throw new KeyNotFoundException("Parcela nije pronađena.");
        if (parcel.OwnerId != request.OwnerId) throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");

        parcel.SetCrop(
            request.CropName,
            request.FloweringStart,
            request.FloweringEnd,
            request.CropNotes);

        await parcelRepository.UpdateAsync(parcel, ct);
    }
}
