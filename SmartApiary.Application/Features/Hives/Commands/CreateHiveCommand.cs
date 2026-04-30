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
    string Description,
    Guid ApiaryId) : IRequest;

public class CreateHiveCommandValidator : AbstractValidator<CreateHiveCommand>
{
    public CreateHiveCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.ApiaryId).NotEmpty();
    }
}

public class CreateHiveCommandHandler(
    IHiveRepository hiveRepository) : IRequestHandler<CreateHiveCommand>
{
    public async Task Handle(CreateHiveCommand request, CancellationToken ct)
    {
        var hive = Hive.Create(
            request.Name,
            request.Description,
            request.ApiaryId);

        await hiveRepository.SaveAsync(hive, ct);
    }
}