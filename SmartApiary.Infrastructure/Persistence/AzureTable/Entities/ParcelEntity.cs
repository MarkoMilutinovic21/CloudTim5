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
}