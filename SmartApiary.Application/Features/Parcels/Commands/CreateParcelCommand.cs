namespace SmartApiary.Application.Features.Parcels.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateParcelCommand(
    string Name,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string Description,
    Guid OwnerId) : IRequest<Guid>;

public class CreateParcelCommandValidator : AbstractValidator<CreateParcelCommand>
{
    public CreateParcelCommandValidator()
    {
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

public class CreateParcelCommandHandler(
    IParcelRepository parcelRepository) : IRequestHandler<CreateParcelCommand, Guid>
{
    public async Task<Guid> Handle(CreateParcelCommand request, CancellationToken ct)
    {
        Parcel parcel = Parcel.Create(
            request.Name,
            request.Area,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.Description,
            request.OwnerId);

        await parcelRepository.SaveAsync(parcel, ct);

        return parcel.Id;
    }
}