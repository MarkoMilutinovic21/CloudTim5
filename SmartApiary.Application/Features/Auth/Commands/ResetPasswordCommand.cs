namespace SmartApiary.Application.Features.Auth.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record ResetPasswordCommand(string Token, string Password, string ConfirmPassword) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Lozinka mora imati najmanje 8 karaktera.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithMessage("Lozinke se ne poklapaju.");
    }
}

public class ResetPasswordCommandHandler(
    IUserRepository userRepository) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByResetPasswordTokenAsync(request.Token, ct);

        if (user is null || !user.IsResetPasswordTokenValid(request.Token))
            throw new Exception("Token nije validan ili je istekao.");

        user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(request.Password));
        await userRepository.UpdateAsync(user, ct);
    }
}