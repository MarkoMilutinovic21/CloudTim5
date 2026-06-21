namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Parcel : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public double Area { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CropName { get; private set; } = string.Empty;
    public DateTime? FloweringStart { get; private set; }
    public DateTime? FloweringEnd { get; private set; }
    public string CropNotes { get; private set; } = string.Empty;

    private Parcel() { }

    public static Parcel Create(
        string name,
        double area,
        string location,
        double latitude,
        double longitude,
        string description,
        Guid ownerId)
    {
        return new Parcel
        {
            Name = name,
            Area = area,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            Description = description,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Parcel Rehydrate(
        Guid id,
        string name,
        double area,
        string location,
        double latitude,
        double longitude,
        string description,
        Guid ownerId,
        DateTime createdAt,
        string cropName = "",
        DateTime? floweringStart = null,
        DateTime? floweringEnd = null,
        string cropNotes = "")
    {
        return new Parcel
        {
            Id = id,
            Name = name,
            Area = area,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            Description = description,
            OwnerId = ownerId,
            CreatedAt = createdAt,
            CropName = cropName,
            FloweringStart = floweringStart,
            FloweringEnd = floweringEnd,
            CropNotes = cropNotes
        };
    }

    public void Update(
        string name,
        double area,
        string location,
        double latitude,
        double longitude,
        string description)
    {
        Name = name;
        Area = area;
        Location = location;
        Latitude = latitude;
        Longitude = longitude;
        Description = description;
    }

    public void SetCrop(
        string cropName,
        DateTime floweringStart,
        DateTime floweringEnd,
        string cropNotes)
    {
        CropName = cropName;
        FloweringStart = floweringStart;
        FloweringEnd = floweringEnd;
        CropNotes = cropNotes;
    }

    public void ClearCrop()
    {
        CropName = string.Empty;
        FloweringStart = null;
        FloweringEnd = null;
        CropNotes = string.Empty;
    }
}
