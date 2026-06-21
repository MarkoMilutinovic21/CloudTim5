namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class User : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ActivationToken { get; private set; }
    public DateTime? ActivationTokenExpiry { get; private set; }
    public string? ResetPasswordToken { get; private set; }
    public DateTime? ResetPasswordTokenExpiry { get; private set; }
    public double WeightDropThresholdKg { get; private set; } = 10;

    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string role,
        string phone = "")
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            ActivationToken = Guid.NewGuid().ToString("N"),
            ActivationTokenExpiry = DateTime.UtcNow.AddHours(24),
            WeightDropThresholdKg = 10
        };
    }

    public void Suspend() => IsActive = false;

    public void Activate() => IsActive = true;

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        IsActive = true;
        ActivationToken = null;
        ActivationTokenExpiry = null;
    }

    public void SetResetPasswordToken()
    {
        ResetPasswordToken = Guid.NewGuid().ToString("N");
        ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
    }

    public void ResetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        ResetPasswordToken = null;
        ResetPasswordTokenExpiry = null;
    }

    public bool IsActivationTokenValid(string token) =>
        ActivationToken == token && ActivationTokenExpiry > DateTime.UtcNow;

    public bool IsResetPasswordTokenValid(string token) =>
        ResetPasswordToken == token && ResetPasswordTokenExpiry > DateTime.UtcNow;

    public void SetWeightDropThreshold(double thresholdKg)
    {
        if (thresholdKg <= 0 || thresholdKg > 100)
            throw new ArgumentOutOfRangeException(nameof(thresholdKg), "Prag mora biti između 0 i 100 kg.");

        WeightDropThresholdKg = thresholdKg;
    }
}
