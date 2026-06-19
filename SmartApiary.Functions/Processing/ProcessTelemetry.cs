namespace SmartApiary.Functions.Processing;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.Alerts;
using SmartApiary.Domain.Common;
using SmartApiary.Domain.Models;
using SmartApiary.Functions.Models;
using System.Text.Json;

public class ProcessTelemetry(
    ITelemetryMeasurementRepository measurementRepository,
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository,
    IUserRepository userRepository,
    IBeekeeperAlertRepository alertRepository,
    IEmailService emailService,
    ILogger<ProcessTelemetry> logger)
{
    [Function(nameof(ProcessTelemetry))]
    public async Task Run(
        [QueueTrigger("%AzureQueueOptions:TelemetryQueue%", Connection = "AzureWebJobsStorage")]
        string messageText,
        CancellationToken ct)
    {
        TelemetryQueueMessage? message = null;
        TelemetryMeasurement? previousMeasurement = null;

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

            previousMeasurement = await measurementRepository.GetLatestForHiveAsync(message.HiveId, ct);

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

            await TryCreateAlertsAsync(measurement, previousMeasurement, ct);

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

    private async Task TryCreateAlertsAsync(
        TelemetryMeasurement measurement,
        TelemetryMeasurement? previousMeasurement,
        CancellationToken ct)
    {
        bool hasPrevious = previousMeasurement is not null &&
            measurement.MeasuredAt >= previousMeasurement.MeasuredAt &&
            previousMeasurement.WeightKg > 0;

        bool weightDropDetected = hasPrevious &&
            measurement.WeightKg <= previousMeasurement!.WeightKg * 0.7;

        bool lowBatteryDetected = measurement.BatteryPercent < 15 &&
            (previousMeasurement is null || previousMeasurement.BatteryPercent >= 20);

        if (!weightDropDetected && !lowBatteryDetected)
            return;

        Hive? hive = await hiveRepository.GetByIdAsync(measurement.HiveId, ct);

        if (hive is null)
        {
            logger.LogWarning(
                "Hive {HiveId} not found while creating alert.",
                measurement.HiveId);
            return;
        }

        Apiary? apiary = await apiaryRepository.GetByIdAsync(hive.ApiaryId, ct);

        if (apiary is null)
        {
            logger.LogWarning(
                "Apiary {ApiaryId} not found while creating alert for hive {HiveId}.",
                hive.ApiaryId,
                hive.Id);
            return;
        }

        User? beekeeper = await userRepository.GetByIdAsync(apiary.OwnerId, ct);

        if (beekeeper is null ||
            beekeeper.Role != UserRoles.Beekeeper ||
            !beekeeper.IsActive)
        {
            logger.LogWarning(
                "Beekeeper {BeekeeperId} not found or inactive while creating alert.",
                apiary.OwnerId);
            return;
        }

        const string subject = "Hitno upozorenje - Smart Apiary";

        if (weightDropDetected)
        {
            string message = CreateWeightDropMessage(apiary, hive, previousMeasurement!, measurement);

            await BeekeeperAlertHelper.CreateAlertAsync(
                beekeeper,
                alertRepository,
                emailService,
                BeekeeperAlertTypes.WeightDrop,
                subject,
                message,
                ct);
        }

        if (lowBatteryDetected)
        {
            string message = CreateLowBatteryMessage(apiary, hive, measurement);

            await BeekeeperAlertHelper.CreateAlertAsync(
                beekeeper,
                alertRepository,
                emailService,
                BeekeeperAlertTypes.LowBattery,
                subject,
                message,
                ct);
        }
    }

    private static string CreateWeightDropMessage(
        Apiary apiary,
        Hive hive,
        TelemetryMeasurement previousMeasurement,
        TelemetryMeasurement measurement)
    {
        double previousWeight = Math.Round(previousMeasurement.WeightKg, 2);
        double currentWeight = Math.Round(measurement.WeightKg, 2);

        return
            "Poštovani," + Environment.NewLine +
            Environment.NewLine +
            "Vaša košnica je pod rizikom: težina je opala za više od 30% u odnosu na prethodno merenje." +
            Environment.NewLine +
            $"Pčelinjak: {apiary.Name}" + Environment.NewLine +
            $"Košnica: {hive.Name}" + Environment.NewLine +
            $"Prethodno merenje: {previousWeight:0.##} kg" + Environment.NewLine +
            $"Trenutno merenje: {currentWeight:0.##} kg" + Environment.NewLine +
            Environment.NewLine +
            "Smart Apiary";
    }

    private static string CreateLowBatteryMessage(
        Apiary apiary,
        Hive hive,
        TelemetryMeasurement measurement)
    {
        double batteryPercent = Math.Round(measurement.BatteryPercent, 1);

        return
            "Poštovani," + Environment.NewLine +
            Environment.NewLine +
            "Vaša košnica je pod rizikom: baterija pametne vage je pala ispod 20%." +
            Environment.NewLine +
            $"Pčelinjak: {apiary.Name}" + Environment.NewLine +
            $"Košnica: {hive.Name}" + Environment.NewLine +
            $"Trenutni nivo baterije: {batteryPercent:0.#}%" + Environment.NewLine +
            Environment.NewLine +
            "Smart Apiary";
    }
}
