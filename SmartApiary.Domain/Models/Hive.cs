using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class Hive : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string HiveType { get; private set; } = string.Empty;
    public string ExtensionColor { get; private set; } = string.Empty;
    public int QueenAge { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid ApiaryId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Hive() { }

    public static Hive Create(
        string name,
        string hiveType,
        string extensionColor,
        int queenAge,
        string description,
        Guid apiaryId)
    {
        return new Hive
        {
            Name = name,
            HiveType = hiveType,
            ExtensionColor = extensionColor,
            QueenAge = queenAge,
            Description = description,
            ApiaryId = apiaryId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Hive Rehydrate(
        Guid id,
        string name,
        string hiveType,
        string extensionColor,
        int queenAge,
        string description,
        Guid apiaryId,
        DateTime createdAt)
    {
        return new Hive
        {
            Id = id,
            Name = name,
            HiveType = hiveType,
            ExtensionColor = extensionColor,
            QueenAge = queenAge,
            Description = description,
            ApiaryId = apiaryId,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string name,
        string hiveType,
        string extensionColor,
        int queenAge,
        string description)
    {
        Name = name;
        HiveType = hiveType;
        ExtensionColor = extensionColor;
        QueenAge = queenAge;
        Description = description;
    }
}
