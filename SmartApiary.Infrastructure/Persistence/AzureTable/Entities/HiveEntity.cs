using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class HiveEntity : BaseTableEntity
{
    public string Name { get; set; } = string.Empty;
    public string HiveType { get; set; } = string.Empty;
    public string ExtensionColor { get; set; } = string.Empty;
    public int QueenAge { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ApiaryId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
