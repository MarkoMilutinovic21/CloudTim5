namespace SmartApiary.Application.Features.SprayingRecords.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetSprayingRecordsQuery(Guid ParcelId, Guid FarmerId, DateTime? From, DateTime? To)
    : IRequest<IReadOnlyCollection<SprayingRecord>>;

public class GetSprayingRecordsHandler(
    ISprayingRecordRepository repo,
    IParcelRepository parcelRepository)
    : IRequestHandler<GetSprayingRecordsQuery, IReadOnlyCollection<SprayingRecord>>
{
    public async Task<IReadOnlyCollection<SprayingRecord>> Handle(GetSprayingRecordsQuery request, CancellationToken ct)
    {
        Parcel? parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is null)
            throw new KeyNotFoundException("Parcela nije pronađena.");
        if (parcel.OwnerId != request.FarmerId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");

        return await repo.GetByParcelIdAsync(request.ParcelId, request.From, request.To, ct);
    }
}
