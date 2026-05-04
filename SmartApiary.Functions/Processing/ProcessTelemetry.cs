namespace SmartApiary.Functions.Processing;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Functions.Models;
using System.Text.Json;

public class ProcessTelemetry(
    ITelemetryMeasurementRepository measurementRepository,
    ILogger<ProcessTelemetry> logger)
{
    [Function(nameof(ProcessTelemetry))]
    public async Task Run(
        [QueueTrigger("%AzureQueueOptions:TelemetryQueue%", Connection = "AzureWebJobsStorage")]
        string messageText,
        CancellationToken ct)
    {
        TelemetryQueueMessage? message = null;

        try
        {
            message = JsonSerializer.Deserialize<TelemetryQueueMessage>(
                messageText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (message is null)
                throw new InvalidOperationException("Telemetry queue message is empty or invalid.");

            TelemetryMeasurement measurement = TelemetryMeasurement.Create(
                message.DeviceId,
                message.HiveId,
                message.DeviceUuid,
                message.WeightKg,
                message.TemperatureC,
                message.HumidityPercent,
                message.BatteryPercent,
                message.MeasuredAt);

            await measurementRepository.SaveAsync(measurement, ct);

            logger.LogInformation(
                "Telemetry measurement {MeasurementId} saved for hive {HiveId}.",
                measurement.Id,
                measurement.HiveId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process telemetry message for device {DeviceId} and hive {HiveId}.",
                message?.DeviceId,
                message?.HiveId);

            throw;
        }
    }
}
