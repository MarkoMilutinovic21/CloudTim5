namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class BeekeeperAlert : AggregateRoot
{
    public Guid BeekeeperId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private BeekeeperAlert() { }

    public static BeekeeperAlert Create(
        Guid beekeeperId,
        string type,
        string title,
        string message)
    {
        Validate(beekeeperId, type, title, message);

        return new BeekeeperAlert
        {
            BeekeeperId = beekeeperId,
            Type = type,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static BeekeeperAlert Rehydrate(
        Guid id,
        Guid beekeeperId,
        string type,
        string title,
        string message,
        DateTime createdAt)
    {
        Validate(beekeeperId, type, title, message);

        return new BeekeeperAlert
        {
            Id = id,
            BeekeeperId = beekeeperId,
            Type = type,
            Title = title,
            Message = message,
            CreatedAt = createdAt
        };
    }

    private static void Validate(
        Guid beekeeperId,
        string type,
        string title,
        string message)
    {
        if (beekeeperId == Guid.Empty)
            throw new ArgumentException("Beekeeper id is required.", nameof(beekeeperId));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Alert type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Alert message is required.", nameof(message));
    }
}

public static class BeekeeperAlertTypes
{
    public const string WeightDrop = "WeightDrop";
    public const string LowBattery = "LowBattery";
    public const string PesticideTreatment = "PesticideTreatment";
}
