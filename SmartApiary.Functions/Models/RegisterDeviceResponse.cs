namespace SmartApiary.Functions.Models;

public class RegisterDeviceResponse
{
    public Guid DeviceId { get; set; }
    public Guid HiveId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DeviceToken { get; set; }
}
