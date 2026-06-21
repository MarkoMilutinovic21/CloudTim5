namespace SmartApiary.Application.Features.Parcels.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record UpdateParcelCommand(
    Guid ParcelId,
    string Name,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string Description,
    Guid OwnerId) : IRequest;

public class UpdateParcelCommandValidator : AbstractValidator<UpdateParcelCommand>
{
    public UpdateParcelCommandValidator()
    {
        RuleFor(x => x.ParcelId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Area)
            .GreaterThan(0);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.OwnerId)
            .NotEmpty();
    }
}

public class UpdateParcelCommandHandler(
    IParcelRepository parcelRepository) : IRequestHandler<UpdateParcelCommand>
{
    public async Task Handle(UpdateParcelCommand request, CancellationToken ct)
    {
        var parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);

        if (parcel is null)
        {
            throw new KeyNotFoundException("Parcela nije pronađena.");
        }

        if (parcel.OwnerId != request.OwnerId)
        {
            throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");
        }

        parcel.Update(
            request.Name,
            request.Area,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.Description);

        await parcelRepository.UpdateAsync(parcel, ct);
    }
}