namespace SmartApiary.Application.Features.PesticideTreatments.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetPesticideTreatmentsQuery(Guid FarmerId)
    : IRequest<IReadOnlyCollection<PesticideTreatmentDto>>;

public record PesticideTreatmentDto(
    Guid Id,
    Guid ParcelId,
    string ParcelName,
    DateTime PlannedStartAt,
    double DurationHours,
    string PesticideType,
    string Status,
    int NotifiedBeekeepersCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CancelledAt);

public class GetPesticideTreatmentsQueryHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository)
    : IRequestHandler<GetPesticideTreatmentsQuery, IReadOnlyCollection<PesticideTreatmentDto>>
{
    public async Task<IReadOnlyCollection<PesticideTreatmentDto>> Handle(
        GetPesticideTreatmentsQuery request,
        CancellationToken ct)
    {
        IReadOnlyCollection<PesticideTreatment> treatments =
            await treatmentRepository.GetByFarmerIdAsync(request.FarmerId, ct);

        List<PesticideTreatmentDto> result = new();

        foreach (PesticideTreatment treatment in treatments.OrderByDescending(x => x.PlannedStartAt))
        {
            Parcel? parcel = await parcelRepository.GetByIdAsync(treatment.ParcelId, ct);

            result.Add(new PesticideTreatmentDto(
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

        return result.AsReadOnly();
    }
}