namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class BeekeeperAlertEntity : BaseTableEntity
{
    public string BeekeeperId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
