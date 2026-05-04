namespace SmartApiary.Functions.Models;

public class TelemetryQueueMessage
{
    public Guid DeviceId { get; set; }
    public Guid HiveId { get; set; }
    public Guid DeviceUuid { get; set; }
    public double WeightKg { get; set; }
    public double TemperatureC { get; set; }
    public double HumidityPercent { get; set; }
    public double BatteryPercent { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
}
