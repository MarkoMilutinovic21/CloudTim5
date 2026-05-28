namespace SmartApiary.Application.Features.Auth.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository, IEmailService emailService) : IRequestHandler<ForgotPasswordCommand>

{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null) return; // Ne otkrivamo da li korisnik postoji

        user.SetResetPasswordToken();
        await userRepository.UpdateAsync(user, ct);

        const string subject = "Resetovanje lozinke - Smart Apiary";

        await emailService.SendPasswordResetLinkAsync(
            user.Email,
            subject,
            user.ResetPasswordToken!,
            ct);
    }
}