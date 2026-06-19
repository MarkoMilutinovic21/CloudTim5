namespace SmartApiary.Application.Common.Interfaces;

public interface IWeatherService
{
    Task<double?> GetCurrentWindSpeedAsync(double latitude, double longitude, CancellationToken ct = default);
    Task<string?> GetWeatherWarningAsync(double latitude, double longitude, CancellationToken ct = default);
}
