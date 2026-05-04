using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Infrastructure;

public class AzureTableOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DevicesTable { get; set; } = "Devices";
    public string MeasurementsTable { get; set; } = "Measurements";
}
