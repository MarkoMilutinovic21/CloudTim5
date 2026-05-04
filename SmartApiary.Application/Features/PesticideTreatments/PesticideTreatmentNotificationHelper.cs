namespace SmartApiary.Application.Features.PesticideTreatments;

using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Common;
using SmartApiary.Domain.Models;

public static class PesticideTreatmentNotificationHelper
{
    public static async Task<IReadOnlyCollection<User>> GetNearbyBeekeepersAsync(
        Parcel parcel,
        IApiaryRepository apiaryRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        IReadOnlyCollection<Apiary> apiaries = await apiaryRepository.GetAllAsync(ct);

        List<Guid> beekeeperIds = apiaries
            .Where(apiary => CalculateDistanceKm(
                parcel.Latitude,
                parcel.Longitude,
                apiary.Latitude,
                apiary.Longitude) <= 5)
            .Select(apiary => apiary.OwnerId)
            .Distinct()
            .ToList();

        List<User> beekeepers = new();

        foreach (Guid beekeeperId in beekeeperIds)
        {
            User? beekeeper = await userRepository.GetByIdAsync(beekeeperId, ct);

            if (beekeeper is not null &&
                beekeeper.Role == UserRoles.Beekeeper &&
                beekeeper.IsActive)
            {
                beekeepers.Add(beekeeper);
            }
        }

        return beekeepers.AsReadOnly();
    }

    public static async Task<int> TryNotifyBeekeepersAsync(
        IReadOnlyCollection<User> beekeepers,
        IEmailService emailService,
        string subject,
        string message,
        CancellationToken ct)
    {
        int successfullyNotifiedCount = 0;

        foreach (User beekeeper in beekeepers)
        {
            try
            {
                await emailService.SendPesticideTreatmentNotificationAsync(
                    beekeeper.Email,
                    subject,
                    message,
                    ct);

                successfullyNotifiedCount++;
            }
            catch
            {
                // Namerno ne rušimo ceo use-case ako email servis nije podešen
                // ili ako slanje pojedinačnog mejla ne uspe.
            }
        }

        return successfullyNotifiedCount;
    }

    public static string CreateScheduledMessage(
        Parcel parcel,
        DateTime plannedStartAt,
        double durationHours,
        string pesticideType)
    {
        return
            "Poštovani," + Environment.NewLine +
            Environment.NewLine +
            "Najavljeno je tretiranje pesticidima u blizini vašeg pčelinjaka." + Environment.NewLine +
            $"Parcela: {parcel.Name}" + Environment.NewLine +
            $"Lokacija: {parcel.Location}" + Environment.NewLine +
            $"Početak tretiranja: {plannedStartAt:dd.MM.yyyy. HH:mm}" + Environment.NewLine +
            $"Očekivano trajanje: {durationHours} h" + Environment.NewLine +
            $"Preparat: {(string.IsNullOrWhiteSpace(pesticideType) ? "Nije navedeno" : pesticideType)}" + Environment.NewLine +
            Environment.NewLine +
            "Smart Apiary";
    }

    public static string CreateUpdatedMessage(
        Parcel parcel,
        DateTime plannedStartAt,
        double durationHours,
        string pesticideType)
    {
        return
            "Poštovani," + Environment.NewLine +
            Environment.NewLine +
            "Izmenjena je najava tretiranja pesticidima u blizini vašeg pčelinjaka." + Environment.NewLine +
            $"Parcela: {parcel.Name}" + Environment.NewLine +
            $"Lokacija: {parcel.Location}" + Environment.NewLine +
            $"Novi početak tretiranja: {plannedStartAt:dd.MM.yyyy. HH:mm}" + Environment.NewLine +
            $"Očekivano trajanje: {durationHours} h" + Environment.NewLine +
            $"Preparat: {(string.IsNullOrWhiteSpace(pesticideType) ? "Nije navedeno" : pesticideType)}" + Environment.NewLine +
            Environment.NewLine +
            "Smart Apiary";
    }

    public static string CreateCancelledMessage(Parcel parcel)
    {
        return
            "Poštovani," + Environment.NewLine +
            Environment.NewLine +
            "Otkazana je najava tretiranja pesticidima u blizini vašeg pčelinjaka." + Environment.NewLine +
            $"Parcela: {parcel.Name}" + Environment.NewLine +
            $"Lokacija: {parcel.Location}" + Environment.NewLine +
            Environment.NewLine +
            "Smart Apiary";
    }

    private static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371;

        double dLat = ToRadians(latitude2 - latitude1);
        double dLon = ToRadians(longitude2 - longitude1);

        double lat1Rad = ToRadians(latitude1);
        double lat2Rad = ToRadians(latitude2);

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double value)
    {
        return value * Math.PI / 180;
    }
}