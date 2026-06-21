namespace SmartApiary.Application.Common.Interfaces;

public interface IWeatherService
{
    Task<WeatherSnapshot?> GetCurrentAsync(double latitude, double longitude, CancellationToken ct = default);
    Task<WeatherSnapshot?> GetForecastAsync(
        double latitude,
        double longitude,
        DateTime forecastAt,
        CancellationToken ct = default);
    Task<string?> GetWeatherWarningAsync(
        double latitude,
        double longitude,
        DateTime forecastAt,
        CancellationToken ct = default);
}

public record WeatherSnapshot(
    DateTime ObservedAt,
    double WindSpeedMs,
    bool HasPrecipitation,
    string Description);
