namespace SmartApiary.Functions.Models;

public class RegisterDeviceRequest
{
    public string SerialNumber { get; set; } = string.Empty;
    public Guid? HiveId { get; set; }
    public Guid? DeviceUuid { get; set; }
}
