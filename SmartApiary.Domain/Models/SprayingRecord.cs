namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class SprayingRecord : AggregateRoot
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public double DurationHours { get; private set; }
    public string ChemicalName { get; private set; } = string.Empty;
    public Guid ParcelId { get; private set; }
    public Guid? TreatmentId { get; private set; }
    public string ParcelName { get; private set; } = string.Empty;
    public string CropName { get; private set; } = string.Empty;
    public string WeatherDescription { get; private set; } = string.Empty;
    public double? WindSpeedMs { get; private set; }
    public bool HadPrecipitation { get; private set; }

    private SprayingRecord() { }

    public static SprayingRecord Create(
        DateTime startTime,
        double durationHours,
        string chemicalName,
        Guid parcelId) =>
        CreateInternal(
            Guid.NewGuid(), startTime, startTime.AddHours(durationHours), durationHours,
            chemicalName, parcelId, null, string.Empty, string.Empty, string.Empty, null, false);

    public static SprayingRecord CreateFromTreatment(
        PesticideTreatment treatment,
        Parcel parcel,
        string weatherDescription,
        double? windSpeedMs,
        bool hadPrecipitation) =>
        CreateInternal(
            Guid.NewGuid(),
            treatment.PlannedStartAt,
            treatment.PlannedStartAt.AddHours(treatment.DurationHours),
            treatment.DurationHours,
            treatment.PesticideType,
            parcel.Id,
            treatment.Id,
            parcel.Name,
            parcel.CropName,
            weatherDescription,
            windSpeedMs,
            hadPrecipitation);

    public static SprayingRecord Rehydrate(
        Guid id,
        DateTime startTime,
        DateTime endTime,
        double durationHours,
        string chemicalName,
        Guid parcelId,
        Guid? treatmentId,
        string parcelName,
        string cropName,
        string weatherDescription,
        double? windSpeedMs,
        bool hadPrecipitation) =>
        CreateInternal(
            id, startTime, endTime, durationHours, chemicalName, parcelId, treatmentId,
            parcelName, cropName, weatherDescription, windSpeedMs, hadPrecipitation);

    private static SprayingRecord CreateInternal(
        Guid id,
        DateTime startTime,
        DateTime endTime,
        double durationHours,
        string chemicalName,
        Guid parcelId,
        Guid? treatmentId,
        string parcelName,
        string cropName,
        string weatherDescription,
        double? windSpeedMs,
        bool hadPrecipitation) => new()
        {
            Id = id,
            StartTime = startTime,
            EndTime = endTime,
            DurationHours = durationHours,
            ChemicalName = chemicalName,
            ParcelId = parcelId,
            TreatmentId = treatmentId,
            ParcelName = parcelName,
            CropName = cropName,
            WeatherDescription = weatherDescription,
            WindSpeedMs = windSpeedMs,
            HadPrecipitation = hadPrecipitation
        };
}
