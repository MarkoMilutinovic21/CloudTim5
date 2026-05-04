namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class PesticideTreatment : AggregateRoot
{
    public Guid ParcelId { get; private set; }
    public Guid FarmerId { get; private set; }
    public DateTime PlannedStartAt { get; private set; }
    public double DurationHours { get; private set; }
    public string PesticideType { get; private set; }
    public string Status { get; private set; }
    public int NotifiedBeekeepersCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private PesticideTreatment() { }

    public static PesticideTreatment Create(
        Guid parcelId,
        Guid farmerId,
        DateTime plannedStartAt,
        double durationHours,
        string pesticideType,
        int notifiedBeekeepersCount)
    {
        return new PesticideTreatment
        {
            ParcelId = parcelId,
            FarmerId = farmerId,
            PlannedStartAt = plannedStartAt,
            DurationHours = durationHours,
            PesticideType = pesticideType,
            Status = PesticideTreatmentStatuses.Scheduled,
            NotifiedBeekeepersCount = notifiedBeekeepersCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PesticideTreatment Rehydrate(
        Guid id,
        Guid parcelId,
        Guid farmerId,
        DateTime plannedStartAt,
        double durationHours,
        string pesticideType,
        string status,
        int notifiedBeekeepersCount,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? cancelledAt)
    {
        return new PesticideTreatment
        {
            Id = id,
            ParcelId = parcelId,
            FarmerId = farmerId,
            PlannedStartAt = plannedStartAt,
            DurationHours = durationHours,
            PesticideType = pesticideType,
            Status = status,
            NotifiedBeekeepersCount = notifiedBeekeepersCount,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CancelledAt = cancelledAt
        };
    }

    public void Update(
        Guid parcelId,
        DateTime plannedStartAt,
        double durationHours,
        string pesticideType,
        int notifiedBeekeepersCount)
    {
        if (Status == PesticideTreatmentStatuses.Cancelled)
        {
            throw new InvalidOperationException("Otkazana najava ne može da se menja.");
        }

        ParcelId = parcelId;
        PlannedStartAt = plannedStartAt;
        DurationHours = durationHours;
        PesticideType = pesticideType;
        NotifiedBeekeepersCount = notifiedBeekeepersCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PesticideTreatmentStatuses.Cancelled)
        {
            return;
        }

        Status = PesticideTreatmentStatuses.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class PesticideTreatmentStatuses
{
    public const string Scheduled = "Scheduled";
    public const string Cancelled = "Cancelled";
}