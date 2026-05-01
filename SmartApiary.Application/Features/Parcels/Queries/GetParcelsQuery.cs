namespace SmartApiary.Application.Features.Parcels.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetParcelsQuery(Guid OwnerId) : IRequest<IReadOnlyCollection<ParcelDto>>;

public record ParcelDto(
    Guid Id,
    string Name,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string Description,
    Guid OwnerId,
    DateTime CreatedAt);

public class GetParcelsQueryHandler(
    IParcelRepository parcelRepository) : IRequestHandler<GetParcelsQuery, IReadOnlyCollection<ParcelDto>>
{
    public async Task<IReadOnlyCollection<ParcelDto>> Handle(GetParcelsQuery request, CancellationToken ct)
    {
        var parcels = await parcelRepository.GetByOwnerIdAsync(request.OwnerId, ct);

        return parcels.Select(parcel => new ParcelDto(
                parcel.Id,
                parcel.Name,
                parcel.Area,
                parcel.Location,
                parcel.Latitude,
                parcel.Longitude,
                parcel.Description,
                parcel.OwnerId,
                parcel.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}