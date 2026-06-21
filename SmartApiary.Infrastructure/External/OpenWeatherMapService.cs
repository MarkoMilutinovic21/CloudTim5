namespace SmartApiary.Infrastructure.External;

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SmartApiary.Application.Common.Interfaces;

public class OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration) : IWeatherService
{
    private readonly string _apiKey = configuration["OpenWeatherMap:ApiKey"]
        ?? throw new InvalidOperationException("OpenWeatherMap:ApiKey nije konfigurisan.");

    public async Task<WeatherSnapshot?> GetForecastAsync(
        double latitude,
        double longitude,
        DateTime forecastAt,
        CancellationToken ct = default)
    {
        try
        {
            string url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            using HttpResponseMessage response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            DateTime targetUtc = forecastAt.Kind == DateTimeKind.Utc
                ? forecastAt
                : forecastAt.ToUniversalTime();

            JsonElement? closest = doc.RootElement.GetProperty("list")
                .EnumerateArray()
                .Select(item => new
                {
                    Item = item,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).UtcDateTime
                })
                .OrderBy(item => Math.Abs((item.Time - targetUtc).Ticks))
                .Select(item => (JsonElement?)item.Item)
                .FirstOrDefault();

            if (!closest.HasValue) return null;

            JsonElement selected = closest.Value;
            DateTime observedAt = DateTimeOffset
                .FromUnixTimeSeconds(selected.GetProperty("dt").GetInt64()).UtcDateTime;
            if (Math.Abs((observedAt - targetUtc).TotalHours) > 4)
                return null;
            double windSpeed = selected.GetProperty("wind").GetProperty("speed").GetDouble();
            string description = selected.GetProperty("weather")[0].GetProperty("description").GetString() ?? string.Empty;
            string main = selected.GetProperty("weather")[0].GetProperty("main").GetString() ?? string.Empty;
            bool precipitation = main is "Rain" or "Drizzle" or "Thunderstorm" or "Snow";

            return new WeatherSnapshot(observedAt, windSpeed, precipitation, description);
        }
        catch
        {
            return null;
        }
    }

    public async Task<WeatherSnapshot?> GetCurrentAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        try
        {
            using JsonDocument? doc = await FetchWeatherAsync(latitude, longitude, ct);
            if (doc is null) return null;

            JsonElement root = doc.RootElement;
            double windSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble();
            string main = root.GetProperty("weather")[0].GetProperty("main").GetString() ?? string.Empty;
            string description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? string.Empty;
            bool precipitation = main is "Rain" or "Drizzle" or "Thunderstorm" or "Snow";
            DateTime observedAt = root.TryGetProperty("dt", out JsonElement timestamp)
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp.GetInt64()).UtcDateTime
                : DateTime.UtcNow;

            return new WeatherSnapshot(observedAt, windSpeed, precipitation, description);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetWeatherWarningAsync(
        double latitude,
        double longitude,
        DateTime forecastAt,
        CancellationToken ct = default)
    {
        WeatherSnapshot? forecast = await GetForecastAsync(latitude, longitude, forecastAt, ct);
        if (forecast is null) return null;

        List<string> warnings = new();
        if (forecast.WindSpeedMs > 5.0)
            warnings.Add($"jak vetar ({forecast.WindSpeedMs:F1} m/s)");
        if (forecast.HasPrecipitation)
            warnings.Add("padavine");

        return warnings.Count == 0
            ? null
            : $"Loši vremenski uslovi ({string.Join(", ", warnings)}) – preporučuje se pomeranje termina.";
    }

    private async Task<JsonDocument?> FetchWeatherAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        var response = await httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
    }
}
