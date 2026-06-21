namespace SmartApiary.Application.Features.Parcels.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetNearbyCropsQuery(Guid BeekeeperId, double? RadiusKm = null) : IRequest<IReadOnlyCollection<NearbyCropDto>>;

public record NearbyCropDto(
    Guid ParcelId,
    string ParcelName,
    double Area,
    string Location,
    double Latitude,
    double Longitude,
    string CropName,
    DateTime? FloweringStart,
    DateTime? FloweringEnd,
    string CropNotes,
    string FarmerName,
    string FarmerPhone,
    Guid ApiaryId,
    string ApiaryName,
    double DistanceKm);

public class GetNearbyCropsQueryHandler(
    IApiaryRepository apiaryRepository,
    IParcelRepository parcelRepository,
    IUserRepository userRepository) : IRequestHandler<GetNearbyCropsQuery, IReadOnlyCollection<NearbyCropDto>>
{
    public async Task<IReadOnlyCollection<NearbyCropDto>> Handle(GetNearbyCropsQuery request, CancellationToken ct)
    {
        var apiaries = await apiaryRepository.GetByOwnerIdAsync(request.BeekeeperId, ct);
        var parcels = await parcelRepository.GetAllAsync(ct);
        var plantedParcels = parcels
            .Where(parcel => !string.IsNullOrWhiteSpace(parcel.CropName))
            .ToList();

        var farmers = (await userRepository.GetAllAsync(ct))
            .ToDictionary(user => user.Id);

        List<NearbyCropDto> results = new();

        foreach (var parcel in plantedParcels)
        {
            var nearestApiary = apiaries
                .Select(apiary => new
                {
                    Apiary = apiary,
                    DistanceKm = CalculateDistanceKm(
                        apiary.Latitude,
                        apiary.Longitude,
                        parcel.Latitude,
                        parcel.Longitude)
                })
                .OrderBy(item => item.DistanceKm)
                .FirstOrDefault();

            double radiusKm = request.RadiusKm is > 0 ? request.RadiusKm.Value : 5;
            if (nearestApiary is null || nearestApiary.DistanceKm > radiusKm)
            {
                continue;
            }

            farmers.TryGetValue(parcel.OwnerId, out var farmer);

            results.Add(new NearbyCropDto(
                parcel.Id,
                parcel.Name,
                parcel.Area,
                parcel.Location,
                parcel.Latitude,
                parcel.Longitude,
                parcel.CropName,
                parcel.FloweringStart,
                parcel.FloweringEnd,
                parcel.CropNotes,
                farmer is null ? string.Empty : $"{farmer.FirstName} {farmer.LastName}".Trim(),
                farmer?.Phone ?? string.Empty,
                nearestApiary?.Apiary.Id ?? Guid.Empty,
                nearestApiary?.Apiary.Name ?? string.Empty,
                nearestApiary is null ? 0 : Math.Round(nearestApiary.DistanceKm, 2)));
        }

        return results
            .OrderBy(crop => crop.CropName)
            .ThenBy(crop => crop.ParcelName)
            .ToList()
            .AsReadOnly();
    }

    private static double CalculateDistanceKm(
        double firstLatitude,
        double firstLongitude,
        double secondLatitude,
        double secondLongitude)
    {
        const double earthRadiusKm = 6371;

        double dLat = ToRadians(secondLatitude - firstLatitude);
        double dLon = ToRadians(secondLongitude - firstLongitude);
        double lat1 = ToRadians(firstLatitude);
        double lat2 = ToRadians(secondLatitude);

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
