namespace SmartApiary.Application.Features.Auth.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null) return; // Ne otkrivamo da li korisnik postoji

        user.SetResetPasswordToken();
        await userRepository.UpdateAsync(user, ct);

        // TODO (Nemanja): Poslati email sa linkom
        // Link format: http://localhost:5173/reset-password?token={user.ResetPasswordToken}
    }
}