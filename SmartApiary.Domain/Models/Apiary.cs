using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Apiary : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Apiary() { }

    public static Apiary Create(
        string name,
        string description,
        string location,
        double latitude,
        double longitude,
        Guid ownerId)
    {
        return new Apiary
        {
            Name = name,
            Description = description,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, string location, double latitude, double longitude)
    {
        Name = name;
        Description = description;
        Location = location;
        Latitude = latitude;
        Longitude = longitude;
    }
}