using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Users.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null) throw new KeyNotFoundException("Korisnik nije pronađen.");
        await userRepository.DeleteAsync(user, ct);
    }
}