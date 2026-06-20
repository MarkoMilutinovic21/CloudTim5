using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Apiaries.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateApiaryCommand(
    string Name,
    string Description,
    string Location,
    double Latitude,
    double Longitude,
    string ImageUrl,
    string ThumbnailUrl,
    Guid OwnerId) : IRequest;

public class CreateApiaryCommandValidator : AbstractValidator<CreateApiaryCommand>
{
    public CreateApiaryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class CreateApiaryCommandHandler(
    IApiaryRepository apiaryRepository) : IRequestHandler<CreateApiaryCommand>
{
    public async Task Handle(CreateApiaryCommand request, CancellationToken ct)
    {
        var apiary = Apiary.Create(
            request.Name,
            request.Description,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.ImageUrl,
            request.ThumbnailUrl,
            request.OwnerId);

        await apiaryRepository.SaveAsync(apiary, ct);
    }
}
