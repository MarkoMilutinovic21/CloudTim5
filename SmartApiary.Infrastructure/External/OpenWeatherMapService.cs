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
            using var doc = await FetchWeatherAsync(latitude, longitude, ct);
            if (doc is null) return null;

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

    public async Task<string?> GetWeatherWarningAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        try
        {
            using var doc = await FetchWeatherAsync(latitude, longitude, ct);
            if (doc is null) return null;

            double? windSpeed = null;
            if (doc.RootElement.TryGetProperty("wind", out var wind) &&
                wind.TryGetProperty("speed", out var speed))
            {
                windSpeed = speed.GetDouble();
            }

            bool isRaining = false;
            if (doc.RootElement.TryGetProperty("weather", out var weatherArr) &&
                weatherArr.GetArrayLength() > 0)
            {
                var main = weatherArr[0].GetProperty("main").GetString();
                isRaining = main is "Rain" or "Drizzle" or "Thunderstorm";
            }

            var warnings = new List<string>();
            if (windSpeed.HasValue && windSpeed.Value > 5.0)
                warnings.Add($"jak vetar ({windSpeed.Value:F1} m/s)");
            if (isRaining)
                warnings.Add("kiša");

            if (warnings.Count == 0) return null;
            return $"Loši vremenski uslovi ({string.Join(", ", warnings)}) – preporučuje se pomeranje termina.";
        }
        catch
        {
            return null;
        }
    }

    private async Task<JsonDocument?> FetchWeatherAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        var response = await httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
    }
}
