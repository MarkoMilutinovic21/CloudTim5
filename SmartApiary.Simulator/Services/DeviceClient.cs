namespace SmartApiary.Simulator.Services;

using System.Net.Http.Json;
using SmartApiary.Simulator.Models;

public class DeviceClient(HttpClient httpClient)
{
    public async Task<RegisterDeviceResponse> RegisterAsync(
        string serialNumber,
        Guid hiveId,
        CancellationToken ct = default)
    {
        RegisterDeviceRequest request = new()
        {
            SerialNumber = serialNumber,
            HiveId = hiveId
        };

        return await PostRegisterDeviceAsync(request, ct);
    }

    public async Task<RegisterDeviceResponse> HandshakeAsync(
        string serialNumber,
        Guid deviceUuid,
        CancellationToken ct = default)
    {
        RegisterDeviceRequest request = new()
        {
            SerialNumber = serialNumber,
            DeviceUuid = deviceUuid
        };

        return await PostRegisterDeviceAsync(request, ct);
    }

    private async Task<RegisterDeviceResponse> PostRegisterDeviceAsync(
        RegisterDeviceRequest request,
        CancellationToken ct)
    {
        using HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("api/RegisterDevice", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"RegisterDevice failed with {(int)response.StatusCode}: {error}");
        }

        RegisterDeviceResponse? body =
            await response.Content.ReadFromJsonAsync<RegisterDeviceResponse>(cancellationToken: ct);

        return body ?? throw new InvalidOperationException("RegisterDevice returned empty response.");
    }
}
