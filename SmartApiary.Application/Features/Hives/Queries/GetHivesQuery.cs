using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Hives.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetHivesQuery(Guid ApiaryId, Guid OwnerId) : IRequest<IReadOnlyCollection<HiveDto>>;

public record HiveDto(
    Guid Id,
    string Name,
    string HiveType,
    string ExtensionColor,
    int QueenAge,
    string Description,
    Guid ApiaryId,
    DateTime CreatedAt);

public class GetHivesQueryHandler(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : IRequestHandler<GetHivesQuery, IReadOnlyCollection<HiveDto>>
{
    public async Task<IReadOnlyCollection<HiveDto>> Handle(GetHivesQuery request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);
        if (apiary is null) throw new KeyNotFoundException("Pčelinjak nije pronađen.");
        if (apiary.OwnerId != request.OwnerId) throw new UnauthorizedAccessException("Nemate pristup ovom pčelinjaku.");

        var hives = await hiveRepository.GetByApiaryIdAsync(request.ApiaryId, ct);
        return hives.Select(h => new HiveDto(
            h.Id, h.Name, h.HiveType, h.ExtensionColor, h.QueenAge, h.Description, h.ApiaryId, h.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
