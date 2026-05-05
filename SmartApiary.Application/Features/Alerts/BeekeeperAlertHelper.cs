namespace SmartApiary.Application.Features.Alerts;

using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public static class BeekeeperAlertHelper
{
    public static async Task<int> CreateAlertsAsync(
        IReadOnlyCollection<User> beekeepers,
        IBeekeeperAlertRepository alertRepository,
        IEmailService emailService,
        string type,
        string subject,
        string message,
        CancellationToken ct)
    {
        int notifiedCount = 0;

        foreach (User beekeeper in beekeepers)
        {
            await CreateAlertAsync(
                beekeeper,
                alertRepository,
                emailService,
                type,
                subject,
                message,
                ct);

            notifiedCount++;
        }

        return notifiedCount;
    }

    public static async Task CreateAlertAsync(
        User beekeeper,
        IBeekeeperAlertRepository alertRepository,
        IEmailService emailService,
        string type,
        string subject,
        string message,
        CancellationToken ct)
    {
        BeekeeperAlert alert = BeekeeperAlert.Create(
            beekeeper.Id,
            type,
            subject,
            message);

        await alertRepository.SaveAsync(alert, ct);

        await emailService.SendBeekeeperAlertAsync(
            beekeeper.Email,
            subject,
            message,
            ct);
    }
}
