namespace SmartApiary.Application.Features.Users.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Common;

public record UpdateBeekeeperSettingsCommand(Guid UserId, double WeightDropThresholdKg) : IRequest;

public sealed class UpdateBeekeeperSettingsCommandValidator
    : AbstractValidator<UpdateBeekeeperSettingsCommand>
{
    public UpdateBeekeeperSettingsCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.WeightDropThresholdKg)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

public sealed class UpdateBeekeeperSettingsCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateBeekeeperSettingsCommand>
{
    public async Task Handle(UpdateBeekeeperSettingsCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new KeyNotFoundException("Korisnik nije pronađen.");
        if (user.Role != UserRoles.Beekeeper)
            throw new UnauthorizedAccessException("Podešavanje je dostupno samo pčelarima.");

        user.SetWeightDropThreshold(request.WeightDropThresholdKg);
        await userRepository.UpdateAsync(user, ct);
    }
}
