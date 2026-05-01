using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmartApiary.Application.Common.Interfaces;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendActivationEmailAsync(string to, string token, CancellationToken ct = default)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var toAddress = new EmailAddress(to);
            var subject = "Aktivacija naloga - Smart Apiary";

            var activationLink = $"http://localhost:5173/activate?token={token}";

            var plainTextContent = $"Molimo vas da aktivirate svoj nalog klikom na link: {activationLink}";
            var htmlContent = $"<strong>Molimo vas da aktivirate svoj nalog klikom na link:</strong> <a href=\"{activationLink}\">Aktiviraj nalog</a>";

            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent);

            await client.SendEmailAsync(msg, ct);
        }
    }
}
