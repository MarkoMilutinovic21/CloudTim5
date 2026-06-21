using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Hives.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateHiveCommand(
    string Name,
    string HiveType,
    string ExtensionColor,
    int QueenAge,
    string Description,
    Guid ApiaryId,
    Guid OwnerId) : IRequest;

public class CreateHiveCommandValidator : AbstractValidator<CreateHiveCommand>
{
    public CreateHiveCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HiveType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ExtensionColor).NotEmpty().MaximumLength(50);
        RuleFor(x => x.QueenAge).GreaterThanOrEqualTo(0).LessThanOrEqualTo(10);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.ApiaryId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class CreateHiveCommandHandler(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : IRequestHandler<CreateHiveCommand>
{
    public async Task Handle(CreateHiveCommand request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);
        if (apiary is null) throw new KeyNotFoundException("Pčelinjak nije pronađen.");
        if (apiary.OwnerId != request.OwnerId) throw new UnauthorizedAccessException("Nemate pristup ovom pčelinjaku.");

        var hive = Hive.Create(
            request.Name,
            request.HiveType,
            request.ExtensionColor,
            request.QueenAge,
            request.Description,
            request.ApiaryId);

        await hiveRepository.SaveAsync(hive, ct);
    }
}
