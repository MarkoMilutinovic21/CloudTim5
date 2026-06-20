using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Apiary : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Apiary() { }

    public static Apiary Create(
        string name,
        string description,
        string location,
        double latitude,
        double longitude,
        string imageUrl,
        string thumbnailUrl,
        Guid ownerId)
    {
        return new Apiary
        {
            Name = name,
            Description = description,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Apiary Rehydrate(
        Guid id,
        string name,
        string description,
        string location,
        double latitude,
        double longitude,
        string imageUrl,
        string thumbnailUrl,
        Guid ownerId,
        DateTime createdAt)
    {
        return new Apiary
        {
            Id = id,
            Name = name,
            Description = description,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            OwnerId = ownerId,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string name,
        string description,
        string location,
        double latitude,
        double longitude,
        string imageUrl,
        string thumbnailUrl)
    {
        Name = name;
        Description = description;
        Location = location;
        Latitude = latitude;
        Longitude = longitude;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
    }
}
