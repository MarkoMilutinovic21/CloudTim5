namespace SmartApiary.WebApi.Services;

using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public sealed class SprayingRecordAutomationService(
    IServiceProvider serviceProvider,
    ILogger<SprayingRecordAutomationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                await ProcessTreatmentsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Automatska obrada kartona prskanja nije uspela.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessTreatmentsAsync(CancellationToken ct)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var treatmentRepository = scope.ServiceProvider.GetRequiredService<IPesticideTreatmentRepository>();
        var sprayingRecordRepository = scope.ServiceProvider.GetRequiredService<ISprayingRecordRepository>();
        var parcelRepository = scope.ServiceProvider.GetRequiredService<IParcelRepository>();
        var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();

        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<PesticideTreatment> treatments = await treatmentRepository.GetAllAsync(ct);
        foreach (PesticideTreatment treatment in treatments.Where(t =>
                     t.Status == PesticideTreatmentStatuses.Scheduled && t.PlannedStartAt <= now))
        {
            Parcel? parcel = await parcelRepository.GetByIdAsync(treatment.ParcelId, ct);
            if (parcel is null)
            {
                logger.LogWarning("Parcela {ParcelId} nije pronađena za tretman {TreatmentId}.",
                    treatment.ParcelId, treatment.Id);
                continue;
            }

            if (!treatment.WeatherObservedAt.HasValue)
            {
                WeatherSnapshot? weather = await weatherService.GetCurrentAsync(
                    parcel.Latitude, parcel.Longitude, ct);
                if (weather is not null)
                {
                    treatment.CaptureWeather(
                        weather.ObservedAt,
                        weather.Description,
                        weather.WindSpeedMs,
                        weather.HasPrecipitation);
                    await treatmentRepository.UpdateAsync(treatment, ct);
                }
            }

            DateTime treatmentEnd = treatment.PlannedStartAt.AddHours(treatment.DurationHours);
            if (treatmentEnd > now)
                continue;

            SprayingRecord? existing =
                await sprayingRecordRepository.GetByTreatmentIdAsync(treatment.Id, ct);
            if (existing is null)
            {
                SprayingRecord record = SprayingRecord.CreateFromTreatment(
                    treatment,
                    parcel,
                    treatment.WeatherDescription,
                    treatment.WindSpeedMs,
                    treatment.HadPrecipitation);
                await sprayingRecordRepository.SaveAsync(record, ct);
            }

            treatment.Complete();
            await treatmentRepository.UpdateAsync(treatment, ct);
        }
    }
}
