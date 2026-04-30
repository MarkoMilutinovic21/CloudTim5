using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Hive : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid ApiaryId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Hive() { }

    public static Hive Create(
        string name,
        string description,
        Guid apiaryId)
    {
        return new Hive
        {
            Name = name,
            Description = description,
            ApiaryId = apiaryId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}