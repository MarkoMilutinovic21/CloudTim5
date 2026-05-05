namespace SmartApiary.Application.Features.PesticideTreatments.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetPesticideTreatmentNotificationStatusesQuery(Guid FarmerId)
    : IRequest<PesticideTreatmentNotificationStatusOverviewDto>;

public record PesticideTreatmentNotificationStatusOverviewDto(
    int TotalTreatments,
    int ScheduledTreatments,
    int CancelledTreatments,
    int TotalNotifiedBeekeepers,
    IReadOnlyCollection<PesticideTreatmentNotificationStatusDto> Items);

public record PesticideTreatmentNotificationStatusDto(
    Guid TreatmentId,
    Guid ParcelId,
    string ParcelName,
    DateTime PlannedStartAt,
    double DurationHours,
    string PesticideType,
    string TreatmentStatus,
    int NotifiedBeekeepersCount,
    DateTime NotificationCreatedAt,
    DateTime? NotificationUpdatedAt,
    DateTime? CancelledAt);

public class GetPesticideTreatmentNotificationStatusesQueryHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository)
    : IRequestHandler<GetPesticideTreatmentNotificationStatusesQuery, PesticideTreatmentNotificationStatusOverviewDto>
{
    public async Task<PesticideTreatmentNotificationStatusOverviewDto> Handle(
        GetPesticideTreatmentNotificationStatusesQuery request,
        CancellationToken ct)
    {
        IReadOnlyCollection<PesticideTreatment> treatments =
            await treatmentRepository.GetByFarmerIdAsync(request.FarmerId, ct);

        List<PesticideTreatmentNotificationStatusDto> items = new();

        foreach (PesticideTreatment treatment in treatments.OrderByDescending(x => x.CreatedAt))
        {
            Parcel? parcel = await parcelRepository.GetByIdAsync(treatment.ParcelId, ct);

            items.Add(new PesticideTreatmentNotificationStatusDto(
                treatment.Id,
                treatment.ParcelId,
                parcel?.Name ?? "Nepoznata parcela",
                treatment.PlannedStartAt,
                treatment.DurationHours,
                treatment.PesticideType,
                treatment.Status,
                treatment.NotifiedBeekeepersCount,
                treatment.CreatedAt,
                treatment.UpdatedAt,
                treatment.CancelledAt));
        }

        return new PesticideTreatmentNotificationStatusOverviewDto(
            treatments.Count,
            treatments.Count(x => x.Status == PesticideTreatmentStatuses.Scheduled),
            treatments.Count(x => x.Status == PesticideTreatmentStatuses.Cancelled),
            treatments.Sum(x => x.NotifiedBeekeepersCount),
            items.AsReadOnly());
    }
}
