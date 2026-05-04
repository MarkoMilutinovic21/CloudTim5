namespace SmartApiary.Functions.Http;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;
using SmartApiary.Functions.Models;

public class RegisterDevice(
    IDeviceRepository deviceRepository,
    ILogger<RegisterDevice> logger)
{
    [Function(nameof(RegisterDevice))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken ct)
    {
        RegisterDeviceRequest? request = await req.ReadFromJsonAsync<RegisterDeviceRequest>(ct);

        if (request is null)
            return new BadRequestObjectResult(new { error = "Invalid or empty JSON payload." });

        if (string.IsNullOrWhiteSpace(request.SerialNumber))
            return new BadRequestObjectResult(new { error = "SerialNumber is required." });

        Device? existing = await deviceRepository.GetBySerialNumberAsync(request.SerialNumber, ct);

        if (request.DeviceUuid.HasValue)
            return await PairDeviceAsync(request, existing, ct);

        if (!request.HiveId.HasValue || request.HiveId.Value == Guid.Empty)
            return new BadRequestObjectResult(new { error = "HiveId is required for device registration." });

        if (existing is not null)
            return new ConflictObjectResult(new { error = "Device with this serial number is already registered." });

        try
        {
            Device device = Device.Create(request.SerialNumber, request.HiveId.Value);
            await deviceRepository.SaveAsync(device, ct);

            logger.LogInformation("Device {SerialNumber} registered for hive {HiveId}.",
                device.SerialNumber, device.HiveId);

            return new OkObjectResult(ToResponse(device));
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    private async Task<IActionResult> PairDeviceAsync(
        RegisterDeviceRequest request,
        Device? existing,
        CancellationToken ct)
    {
        if (existing is null)
            return new NotFoundObjectResult(new { error = "Device serial number is not registered." });

        if (existing.Status == DevicePairingStatus.Paired)
            return new ConflictObjectResult(new { error = "Device is already paired." });

        string token = GenerateDeviceToken();
        existing.Pair(request.DeviceUuid!.Value, token);

        await deviceRepository.UpdateAsync(existing, ct);

        logger.LogInformation("Device {SerialNumber} paired with UUID {DeviceUuid}.",
            existing.SerialNumber, existing.DeviceUuid);

        return new OkObjectResult(ToResponse(existing));
    }

    private static RegisterDeviceResponse ToResponse(Device device)
    {
        return new RegisterDeviceResponse
        {
            DeviceId = device.Id,
            HiveId = device.HiveId,
            SerialNumber = device.SerialNumber,
            Status = device.Status,
            DeviceToken = device.DeviceToken
        };
    }

    private static string GenerateDeviceToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
