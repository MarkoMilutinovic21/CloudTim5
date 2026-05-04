namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class TelemetryMeasurement : Entity
{
    public Guid DeviceId { get; private set; }
    public Guid HiveId { get; private set; }
    public Guid DeviceUuid { get; private set; }
    public double WeightKg { get; private set; }
    public double TemperatureC { get; private set; }
    public double HumidityPercent { get; private set; }
    public double BatteryPercent { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    private TelemetryMeasurement() { }

    public static TelemetryMeasurement Create(
        Guid deviceId,
        Guid hiveId,
        Guid deviceUuid,
        double weightKg,
        double temperatureC,
        double humidityPercent,
        double batteryPercent,
        DateTime measuredAt)
    {
        Validate(deviceId, hiveId, deviceUuid, humidityPercent, batteryPercent);

        return new TelemetryMeasurement
        {
            DeviceId = deviceId,
            HiveId = hiveId,
            DeviceUuid = deviceUuid,
            WeightKg = weightKg,
            TemperatureC = temperatureC,
            HumidityPercent = humidityPercent,
            BatteryPercent = batteryPercent,
            MeasuredAt = measuredAt,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public static TelemetryMeasurement Load(
        Guid id,
        Guid deviceId,
        Guid hiveId,
        Guid deviceUuid,
        double weightKg,
        double temperatureC,
        double humidityPercent,
        double batteryPercent,
        DateTime measuredAt,
        DateTime receivedAt)
    {
        Validate(deviceId, hiveId, deviceUuid, humidityPercent, batteryPercent);

        return new TelemetryMeasurement
        {
            Id = id,
            DeviceId = deviceId,
            HiveId = hiveId,
            DeviceUuid = deviceUuid,
            WeightKg = weightKg,
            TemperatureC = temperatureC,
            HumidityPercent = humidityPercent,
            BatteryPercent = batteryPercent,
            MeasuredAt = measuredAt,
            ReceivedAt = receivedAt
        };
    }

    private static void Validate(
        Guid deviceId,
        Guid hiveId,
        Guid deviceUuid,
        double humidityPercent,
        double batteryPercent)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device id is required.", nameof(deviceId));

        if (hiveId == Guid.Empty)
            throw new ArgumentException("Hive id is required.", nameof(hiveId));

        if (deviceUuid == Guid.Empty)
            throw new ArgumentException("Device UUID is required.", nameof(deviceUuid));

        if (humidityPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(humidityPercent), "Humidity must be between 0 and 100.");

        if (batteryPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(batteryPercent), "Battery must be between 0 and 100.");
    }
}
