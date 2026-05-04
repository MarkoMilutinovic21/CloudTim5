namespace SmartApiary.Simulator.Models;

public class SimulatorState
{
    public string SerialNumber { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public Guid HiveId { get; set; }
    public Guid DeviceUuid { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
}
