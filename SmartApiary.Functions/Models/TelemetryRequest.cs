namespace SmartApiary.Functions.Models;

public class TelemetryRequest
{
    public Guid DeviceUuid { get; set; }
    public double WeightKg { get; set; }
    public double TemperatureC { get; set; }
    public double HumidityPercent { get; set; }
    public double BatteryPercent { get; set; }
    public DateTime? MeasuredAt { get; set; }
}
