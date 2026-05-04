namespace SmartApiary.Domain.Models;

using System.Text.RegularExpressions;
using SmartApiary.Domain.Common;

public class Device : AggregateRoot
{
    private static readonly Regex SerialNumberPattern = new(
        "^SA-[0-9]{4}-[0-9]{5}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string SerialNumber { get; private set; } = string.Empty;
    public Guid HiveId { get; private set; }
    public Guid? DeviceUuid { get; private set; }
    public string? DeviceToken { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime RegisteredAt { get; private set; }
    public DateTime? PairedAt { get; private set; }

    private Device() { }

    public static Device Create(string serialNumber, Guid hiveId)
    {
        ValidateSerialNumber(serialNumber);

        return new Device
        {
            SerialNumber = serialNumber,
            HiveId = hiveId,
            Status = DevicePairingStatus.Unpaired,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public static Device Load(
        Guid id,
        string serialNumber,
        Guid hiveId,
        Guid? deviceUuid,
        string? deviceToken,
        string status,
        DateTime registeredAt,
        DateTime? pairedAt)
    {
        ValidateSerialNumber(serialNumber);

        return new Device
        {
            Id = id,
            SerialNumber = serialNumber,
            HiveId = hiveId,
            DeviceUuid = deviceUuid,
            DeviceToken = deviceToken,
            Status = status,
            RegisteredAt = registeredAt,
            PairedAt = pairedAt
        };
    }

    public void Pair(Guid deviceUuid, string deviceToken)
    {
        if (deviceUuid == Guid.Empty)
            throw new ArgumentException("Device UUID is required.", nameof(deviceUuid));

        if (string.IsNullOrWhiteSpace(deviceToken))
            throw new ArgumentException("Device token is required.", nameof(deviceToken));

        DeviceUuid = deviceUuid;
        DeviceToken = deviceToken;
        Status = DevicePairingStatus.Paired;
        PairedAt = DateTime.UtcNow;
    }

    public bool IsTokenValid(string token) =>
        Status == DevicePairingStatus.Paired &&
        !string.IsNullOrWhiteSpace(DeviceToken) &&
        DeviceToken == token;

    private static void ValidateSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber) || !SerialNumberPattern.IsMatch(serialNumber))
            throw new ArgumentException("Serial number must be in SA-YYYY-XXXXX format.", nameof(serialNumber));
    }
}
