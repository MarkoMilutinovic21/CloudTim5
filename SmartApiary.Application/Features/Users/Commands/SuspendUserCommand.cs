using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Users.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record SuspendUserCommand(Guid UserId) : IRequest;

public class SuspendUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<SuspendUserCommand>
{
    public async Task Handle(SuspendUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null) throw new Exception("Korisnik nije pronađen.");
        user.Suspend();
        await userRepository.UpdateAsync(user, ct);
    }
}