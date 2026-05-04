namespace SmartApiary.Functions.Options;

public class AzureQueueOptions
{
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
    public string TelemetryQueue { get; set; } = "telemetry-queue";
}
