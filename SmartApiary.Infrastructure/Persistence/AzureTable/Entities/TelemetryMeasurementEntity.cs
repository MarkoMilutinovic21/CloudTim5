namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class TelemetryMeasurementEntity : BaseTableEntity
{
    public string DeviceId { get; set; } = string.Empty;
    public string HiveId { get; set; } = string.Empty;
    public string DeviceUuid { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public double TemperatureC { get; set; }
    public double HumidityPercent { get; set; }
    public double BatteryPercent { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
}
