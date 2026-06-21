using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Users.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record ActivateAccountCommand(string Token, string Password, string ConfirmPassword) : IRequest;

public class ActivateAccountCommandValidator : AbstractValidator<ActivateAccountCommand>
{
    public ActivateAccountCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Lozinka mora imati najmanje 8 karaktera.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithMessage("Lozinke se ne poklapaju.");
    }
}

public class ActivateAccountCommandHandler(
    IUserRepository userRepository) : IRequestHandler<ActivateAccountCommand>
{
    public async Task Handle(ActivateAccountCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByActivationTokenAsync(request.Token, ct);

        if (user is null || !user.IsActivationTokenValid(request.Token))
            throw new InvalidOperationException("Token nije validan ili je istekao.");

        user.SetPassword(BCrypt.Net.BCrypt.HashPassword(request.Password));
        await userRepository.UpdateAsync(user, ct);
    }
}