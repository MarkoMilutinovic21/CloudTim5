using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Crops.Queries;

using MediatR;
using SmartApiary.Domain.Models;
using SmartApiary.Application.Common.Interfaces;

public record GetCropsQuery(Guid ParcelId, Guid FarmerId) : IRequest<IReadOnlyCollection<Crop>>;

public class GetCropsQueryHandler(
    ICropRepository cropRepository,
    IParcelRepository parcelRepository
) : IRequestHandler<GetCropsQuery, IReadOnlyCollection<Crop>>
{
    public async Task<IReadOnlyCollection<Crop>> Handle(GetCropsQuery request, CancellationToken ct)
    {
        Parcel? parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is null)
            throw new KeyNotFoundException("Parcela nije pronađena.");
        if (parcel.OwnerId != request.FarmerId)
            throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");

        return await cropRepository.GetByParcelIdAsync(request.ParcelId, ct);
    }
}
