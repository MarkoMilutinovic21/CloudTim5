namespace SmartApiary.Application.Features.Users.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Common;

public record GetBeekeeperSettingsQuery(Guid UserId) : IRequest<BeekeeperSettingsDto>;
public record BeekeeperSettingsDto(double WeightDropThresholdKg);

public sealed class GetBeekeeperSettingsQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetBeekeeperSettingsQuery, BeekeeperSettingsDto>
{
    public async Task<BeekeeperSettingsDto> Handle(GetBeekeeperSettingsQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new KeyNotFoundException("Korisnik nije pronađen.");
        if (user.Role != UserRoles.Beekeeper)
            throw new UnauthorizedAccessException("Podešavanje je dostupno samo pčelarima.");

        return new BeekeeperSettingsDto(
            user.WeightDropThresholdKg > 0 ? user.WeightDropThresholdKg : 10);
    }
}
