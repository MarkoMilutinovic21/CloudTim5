namespace SmartApiary.Application.Features.SprayingRecords.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record GetSprayingRecordsQuery(Guid ParcelId)
    : IRequest<IReadOnlyCollection<SprayingRecord>>;

public class GetSprayingRecordsHandler(
    ISprayingRecordRepository repo
) : IRequestHandler<GetSprayingRecordsQuery, IReadOnlyCollection<SprayingRecord>>
{
    public async Task<IReadOnlyCollection<SprayingRecord>> Handle(GetSprayingRecordsQuery request, CancellationToken ct)
    {
        return await repo.GetByParcelIdAsync(request.ParcelId, ct);
    }
}