namespace SmartApiary.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmartApiary.Application.Common.Interfaces;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendActivationEmailAsync(string to, string token, CancellationToken ct = default)
    {
        string? apiKey = _configuration["SendGrid:ApiKey"];
        string? fromEmail = _configuration["SendGrid:FromEmail"];
        string? fromName = _configuration["SendGrid:FromName"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            return;
        }

        SendGridClient client = new SendGridClient(apiKey);
        EmailAddress from = new EmailAddress(fromEmail, fromName);
        EmailAddress toAddress = new EmailAddress(to);

        string subject = "Aktivacija naloga - Smart Apiary";
        string activationLink = $"http://localhost:5173/activate?token={token}";

        string plainTextContent = $"Molimo vas da aktivirate svoj nalog klikom na link: {activationLink}";
        string htmlContent =
            $"<strong>Molimo vas da aktivirate svoj nalog klikom na link:</strong> " +
            $"<a href=\"{activationLink}\">Aktiviraj nalog</a>";

        SendGridMessage msg = MailHelper.CreateSingleEmail(
            from,
            toAddress,
            subject,
            plainTextContent,
            htmlContent);

        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendPesticideTreatmentNotificationAsync(
        string to,
        string subject,
        string message,
        CancellationToken ct = default)
    {
        await SendEmailAsync(to, subject, message, ct);
    }

    public async Task SendBeekeeperAlertAsync(
        string to,
        string subject,
        string message,
        CancellationToken ct = default)
    {
        await SendEmailAsync(to, subject, message, ct);
    }

    private async Task SendEmailAsync(
        string to,
        string subject,
        string message,
        CancellationToken ct)
    {
        string? apiKey = _configuration["SendGrid:ApiKey"];
        string? fromEmail = _configuration["SendGrid:FromEmail"];
        string? fromName = _configuration["SendGrid:FromName"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            return;
        }

        SendGridClient client = new SendGridClient(apiKey);
        EmailAddress from = new EmailAddress(fromEmail, fromName);
        EmailAddress toAddress = new EmailAddress(to);

        string htmlContent = message.Replace(Environment.NewLine, "<br />");

        SendGridMessage msg = MailHelper.CreateSingleEmail(
            from,
            toAddress,
            subject,
            message,
            htmlContent);

        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendPasswordResetLinkAsync(
     string to,
     string subject,
     string message,
     CancellationToken ct = default)
    {
        string? apiKey = _configuration["SendGrid:ApiKey"];
        string? fromEmail = _configuration["SendGrid:FromEmail"];
        string? fromName = _configuration["SendGrid:FromName"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            return;
        }

        SendGridClient client = new SendGridClient(apiKey);
        EmailAddress from = new EmailAddress(fromEmail, fromName);
        EmailAddress toAddress = new EmailAddress(to);

        string resetLink = $"http://localhost:5173/reset-password?token={message}";

        string plainTextContent = resetLink;
        string htmlContent = $"<a href=\"{resetLink}\">{resetLink}</a>";

        SendGridMessage msg = MailHelper.CreateSingleEmail(
            from,
            toAddress,
            subject,
            plainTextContent,
            htmlContent);

        await client.SendEmailAsync(msg, ct);
    }
}
