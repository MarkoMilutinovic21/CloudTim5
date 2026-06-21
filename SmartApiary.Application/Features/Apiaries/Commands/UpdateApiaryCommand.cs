namespace SmartApiary.Application.Features.Apiaries.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record UpdateApiaryCommand(
    Guid ApiaryId,
    string Name,
    string Description,
    string Location,
    double Latitude,
    double Longitude,
    string ImageUrl,
    string ThumbnailUrl,
    Guid OwnerId) : IRequest;

public class UpdateApiaryCommandValidator : AbstractValidator<UpdateApiaryCommand>
{
    public UpdateApiaryCommandValidator()
    {
        RuleFor(x => x.ApiaryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class UpdateApiaryCommandHandler(
    IApiaryRepository apiaryRepository) : IRequestHandler<UpdateApiaryCommand>
{
    public async Task Handle(UpdateApiaryCommand request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);

        if (apiary is null)
            throw new KeyNotFoundException("Pčelinjak nije pronađen.");

        if (apiary.OwnerId != request.OwnerId)
            throw new UnauthorizedAccessException("Nemate pristup ovom pčelinjaku.");

        apiary.Update(
            request.Name,
            request.Description,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.ImageUrl,
            request.ThumbnailUrl);

        await apiaryRepository.UpdateAsync(apiary, ct);
    }
}
