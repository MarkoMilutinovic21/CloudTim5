namespace SmartApiary.Simulator.Configuration;

public class SimulatorSettings
{
    public string FunctionsBaseUrl { get; set; } = "http://localhost:7071";
    public string SerialNumber { get; set; } = "SA-2026-12345";
    public Guid? DeviceUuid { get; set; }
    public Guid? HiveId { get; set; }
    public int DelaySeconds { get; set; } = 5;
    public double InitialWeightKg { get; set; } = 42.5;
    public double WeightVariationKg { get; set; } = 0.35;
    public double InitialBatteryPercent { get; set; } = 95;
}
