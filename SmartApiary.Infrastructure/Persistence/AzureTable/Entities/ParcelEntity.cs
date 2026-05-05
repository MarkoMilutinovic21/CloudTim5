namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class ParcelEntity : BaseTableEntity
{
    public string Name { get; set; } = string.Empty;
    public double Area { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CropName { get; set; } = string.Empty;
    public DateTime? FloweringStart { get; set; }
    public DateTime? FloweringEnd { get; set; }
    public string CropNotes { get; set; } = string.Empty;
}
