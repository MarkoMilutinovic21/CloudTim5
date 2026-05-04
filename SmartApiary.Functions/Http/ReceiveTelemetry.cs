namespace SmartApiary.Functions.Http;

using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Functions.Models;
using SmartApiary.Functions.Options;

public class ReceiveTelemetry(
    IDeviceRepository deviceRepository,
    IOptions<AzureQueueOptions> queueOptions,
    ILogger<ReceiveTelemetry> logger)
{
    private const string DeviceTokenHeader = "X-Device-Token";

    [Function(nameof(ReceiveTelemetry))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken ct)
    {
        string? deviceToken = req.Headers[DeviceTokenHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(deviceToken))
            return new UnauthorizedObjectResult(new { error = $"{DeviceTokenHeader} header is required." });

        TelemetryRequest? request = await req.ReadFromJsonAsync<TelemetryRequest>(ct);

        if (request is null)
            return new BadRequestObjectResult(new { error = "Invalid or empty JSON payload." });

        if (request.DeviceUuid == Guid.Empty)
            return new BadRequestObjectResult(new { error = "DeviceUuid is required." });

        if (request.HumidityPercent is < 0 or > 100)
            return new BadRequestObjectResult(new { error = "HumidityPercent must be between 0 and 100." });

        if (request.BatteryPercent is < 0 or > 100)
            return new BadRequestObjectResult(new { error = "BatteryPercent must be between 0 and 100." });

        var device = await deviceRepository.GetByTokenAsync(deviceToken, ct);

        if (device is null || !device.IsTokenValid(deviceToken))
            return new UnauthorizedObjectResult(new { error = "Invalid device token." });

        if (device.DeviceUuid != request.DeviceUuid)
            return new UnauthorizedObjectResult(new { error = "Device UUID does not match token." });

        TelemetryQueueMessage message = new()
        {
            DeviceId = device.Id,
            HiveId = device.HiveId,
            DeviceUuid = request.DeviceUuid,
            WeightKg = request.WeightKg,
            TemperatureC = request.TemperatureC,
            HumidityPercent = request.HumidityPercent,
            BatteryPercent = request.BatteryPercent,
            MeasuredAt = request.MeasuredAt ?? DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow
        };

        QueueClient queueClient = new(
            queueOptions.Value.ConnectionString,
            queueOptions.Value.TelemetryQueue,
            new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

        await queueClient.CreateIfNotExistsAsync(cancellationToken: ct);

        string json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await queueClient.SendMessageAsync(json, ct);

        logger.LogInformation(
            "Telemetry for device {DeviceId} and hive {HiveId} queued.",
            message.DeviceId,
            message.HiveId);

        return new AcceptedResult();
    }
}
