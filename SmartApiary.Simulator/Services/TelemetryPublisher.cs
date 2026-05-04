namespace SmartApiary.Simulator.Services;

using System.Net.Http.Json;
using SmartApiary.Simulator.Models;

public class TelemetryPublisher(HttpClient httpClient)
{
    private const string DeviceTokenHeader = "X-Device-Token";

    public async Task PublishAsync(
        TelemetryRequest telemetry,
        string deviceToken,
        CancellationToken ct = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "api/ReceiveTelemetry")
        {
            Content = JsonContent.Create(telemetry)
        };

        request.Headers.Add(DeviceTokenHeader, deviceToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"ReceiveTelemetry failed with {(int)response.StatusCode}: {error}");
        }
    }
}
