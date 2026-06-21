namespace SmartApiary.WebApi.Services;

using Microsoft.AspNetCore.SignalR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.WebApi.Hubs;
using System.Collections.Concurrent;

public class TelemetryBroadcastService(
    IServiceProvider serviceProvider,
    IHubContext<TelemetryHub> hubContext) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, DateTime> _lastBroadcast = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(3000, stoppingToken);

            foreach (string hiveIdStr in TelemetryHub.WatchedHives.Keys)
            {
                if (!Guid.TryParse(hiveIdStr, out Guid hiveId))
                    continue;

                try
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    ITelemetryMeasurementRepository repo = scope.ServiceProvider
                        .GetRequiredService<ITelemetryMeasurementRepository>();

                    TelemetryMeasurement? latest = await repo.GetLatestForHiveAsync(hiveId, stoppingToken);

                    if (latest is null)
                        continue;

                    DateTime lastSent = _lastBroadcast.GetValueOrDefault(hiveId, DateTime.MinValue);

                    if (latest.MeasuredAt <= lastSent)
                        continue;

                    _lastBroadcast[hiveId] = latest.MeasuredAt;

                    await hubContext.Clients.Group(hiveIdStr).SendAsync(
                        "NewMeasurement",
                        new
                        {
                            id = latest.Id,
                            deviceId = latest.DeviceId,
                            hiveId = latest.HiveId,
                            deviceUuid = latest.DeviceUuid,
                            weightKg = latest.WeightKg,
                            temperatureC = latest.TemperatureC,
                            humidityPercent = latest.HumidityPercent,
                            batteryPercent = latest.BatteryPercent,
                            measuredAt = latest.MeasuredAt,
                            receivedAt = latest.ReceivedAt
                        },
                        stoppingToken);
                }
                catch
                {
                    // ignorisi greske za individualne kosnice
                }
            }
        }
    }
}
