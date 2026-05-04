namespace SmartApiary.Application.Features.Crops.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateCropCommand(
    string Name,
    DateTime SowingDate,
    Guid ParcelId
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

        RuleFor(x => x.SowingDate)
            .LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public class CreateCropCommandHandler(
    ICropRepository cropRepository
) : IRequestHandler<CreateCropCommand, Guid>
{
    public async Task<Guid> Handle(CreateCropCommand request, CancellationToken ct)
    {
        var crop = Crop.Create(
            request.Name,
            request.SowingDate,
            request.ParcelId
        );

        await cropRepository.SaveAsync(crop, ct);

        return crop.Id;
    }
}