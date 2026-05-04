namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class PesticideTreatmentEntity : BaseTableEntity
{
    public string ParcelId { get; set; } = string.Empty;
    public string FarmerId { get; set; } = string.Empty;
    public DateTime PlannedStartAt { get; set; }
    public double DurationHours { get; set; }
    public string PesticideType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int NotifiedBeekeepersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}