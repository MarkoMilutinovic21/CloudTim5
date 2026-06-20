using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Apiaries.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetApiaresQuery(Guid OwnerId) : IRequest<IReadOnlyCollection<ApiaryDto>>;

public record ApiaryDto(
    Guid Id,
    string Name,
    string Description,
    string Location,
    double Latitude,
    double Longitude,
    string ImageUrl,
    string ThumbnailUrl,
    Guid OwnerId,
    DateTime CreatedAt);

public class GetApiaresQueryHandler(
    IApiaryRepository apiaryRepository) : IRequestHandler<GetApiaresQuery, IReadOnlyCollection<ApiaryDto>>
{
    public async Task<IReadOnlyCollection<ApiaryDto>> Handle(GetApiaresQuery request, CancellationToken ct)
    {
        var apiaries = await apiaryRepository.GetByOwnerIdAsync(request.OwnerId, ct);
        return apiaries.Select(a => new ApiaryDto(
            a.Id, a.Name, a.Description, a.Location,
            a.Latitude, a.Longitude, a.ImageUrl, a.ThumbnailUrl, a.OwnerId, a.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
