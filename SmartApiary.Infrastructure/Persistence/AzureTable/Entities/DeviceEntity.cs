namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class DeviceEntity : BaseTableEntity
{
    public string SerialNumber { get; set; } = string.Empty;
    public string HiveId { get; set; } = string.Empty;
    public string? DeviceUuid { get; set; }
    public string? DeviceToken { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime? PairedAt { get; set; }
}
