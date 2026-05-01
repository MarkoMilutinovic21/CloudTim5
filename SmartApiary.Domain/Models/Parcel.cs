namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Parcel : AggregateRoot
{
    public string Name { get; private set; }
    public double Area { get; private set; }
    public string Location { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

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
        DateTime createdAt)
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
            CreatedAt = createdAt
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
}