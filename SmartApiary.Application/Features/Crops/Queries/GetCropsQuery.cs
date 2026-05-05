using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Crops.Queries;

using MediatR;
using SmartApiary.Domain.Models;
using SmartApiary.Application.Common.Interfaces;

public record GetCropsQuery(Guid ParcelId) : IRequest<IReadOnlyCollection<Crop>>;

public class GetCropsQueryHandler(
    ICropRepository cropRepository
) : IRequestHandler<GetCropsQuery, IReadOnlyCollection<Crop>>
{
    public async Task<IReadOnlyCollection<Crop>> Handle(GetCropsQuery request, CancellationToken ct)
    {
        return await cropRepository.GetByParcelIdAsync(request.ParcelId, ct);
    }
}
