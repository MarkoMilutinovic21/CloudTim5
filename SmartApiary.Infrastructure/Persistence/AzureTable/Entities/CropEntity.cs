using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class CropEntity : BaseTableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime SowingDate { get; set; }
}
