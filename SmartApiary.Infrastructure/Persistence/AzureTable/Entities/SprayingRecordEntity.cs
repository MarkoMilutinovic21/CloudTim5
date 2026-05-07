namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class SprayingRecordEntity : BaseTableEntity
{
    public DateTime StartTime { get; set; }
    public double DurationHours { get; set; }
    public string ChemicalName { get; set; } = string.Empty;
}