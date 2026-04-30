using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Hives.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetHivesQuery(Guid ApiaryId) : IRequest<IReadOnlyCollection<HiveDto>>;

public record HiveDto(
    Guid Id,
    string Name,
    string Description,
    Guid ApiaryId,
    DateTime CreatedAt);

public class GetHivesQueryHandler(
    IHiveRepository hiveRepository) : IRequestHandler<GetHivesQuery, IReadOnlyCollection<HiveDto>>
{
    public async Task<IReadOnlyCollection<HiveDto>> Handle(GetHivesQuery request, CancellationToken ct)
    {
        var hives = await hiveRepository.GetByApiaryIdAsync(request.ApiaryId, ct);
        return hives.Select(h => new HiveDto(
            h.Id, h.Name, h.Description, h.ApiaryId, h.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}