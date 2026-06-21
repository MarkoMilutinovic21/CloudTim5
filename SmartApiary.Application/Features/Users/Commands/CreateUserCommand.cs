using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Users.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string Role) : IRequest;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Role).NotEmpty().Must(r => r == "Beekeeper" || r == "Farmer")
            .WithMessage("Uloga mora biti Beekeeper ili Farmer.");
    }
}

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IEmailService emailService) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken ct)
    {
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            string.Empty,
            request.Role,
            request.Phone);

        await userRepository.SaveAsync(user, ct);
        await emailService.SendActivationEmailAsync(user.Email, user.ActivationToken!, ct);
    }
}
