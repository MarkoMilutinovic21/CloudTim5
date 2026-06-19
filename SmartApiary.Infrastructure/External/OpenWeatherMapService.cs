namespace SmartApiary.Infrastructure.External;

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SmartApiary.Application.Common.Interfaces;

public class OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration) : IWeatherService
{
    private readonly string _apiKey = configuration["OpenWeatherMap:ApiKey"]
        ?? throw new InvalidOperationException("OpenWeatherMap:ApiKey nije konfigurisan.");

    public async Task<double?> GetCurrentWindSpeedAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            if (doc.RootElement.TryGetProperty("wind", out var wind) &&
                wind.TryGetProperty("speed", out var speed))
            {
                return speed.GetDouble();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
