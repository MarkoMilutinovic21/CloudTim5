namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class SprayingRecordEntity : BaseTableEntity
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DurationHours { get; set; }
    public string ChemicalName { get; set; } = string.Empty;
    public string TreatmentId { get; set; } = string.Empty;
    public string ParcelName { get; set; } = string.Empty;
    public string CropName { get; set; } = string.Empty;
    public string WeatherDescription { get; set; } = string.Empty;
    public double? WindSpeedMs { get; set; }
    public bool HadPrecipitation { get; set; }
}
