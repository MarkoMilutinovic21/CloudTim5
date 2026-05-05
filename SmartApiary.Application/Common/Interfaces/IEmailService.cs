namespace SmartApiary.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendActivationEmailAsync(string to, string token, CancellationToken ct = default);

    Task SendPesticideTreatmentNotificationAsync(
        string to,
        string subject,
        string message,
        CancellationToken ct = default);

    Task SendBeekeeperAlertAsync(
        string to,
        string subject,
        string message,
        CancellationToken ct = default);
}
