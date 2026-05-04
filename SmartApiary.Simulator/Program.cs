using Microsoft.Extensions.Configuration;
using SmartApiary.Simulator.Configuration;
using SmartApiary.Simulator.Models;
using SmartApiary.Simulator.Services;
using System.Text.Json;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

SimulatorSettings settings = configuration
    .GetSection("SimulatorSettings")
    .Get<SimulatorSettings>()
    ?? throw new InvalidOperationException("SimulatorSettings section is missing.");

ValidateSettings(settings);

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

using HttpClient httpClient = new()
{
    BaseAddress = new Uri(NormalizeBaseUrl(settings.FunctionsBaseUrl))
};

DeviceClient deviceClient = new(httpClient);
TelemetryPublisher telemetryPublisher = new(httpClient);
TelemetryGenerator telemetryGenerator = new(settings);

string statePath = Path.Combine(AppContext.BaseDirectory, "device-state.json");
SimulatorState? state = LoadState(statePath, settings.SerialNumber);

Guid hiveId = state?.HiveId ?? settings.HiveId.GetValueOrDefault(Guid.NewGuid());
Guid deviceUuid = state?.DeviceUuid ?? settings.DeviceUuid.GetValueOrDefault(Guid.NewGuid());

PrintHeader(settings, hiveId, deviceUuid);

try
{
    if (state is null)
    {
        RegisterDeviceResponse registered = await RegisterOrContinueAsync(
            deviceClient,
            settings.SerialNumber,
            hiveId,
            cts.Token);

        Console.WriteLine($"[REGISTER] Device {registered.SerialNumber} status: {registered.Status}");

        RegisterDeviceResponse paired = await deviceClient.HandshakeAsync(
            settings.SerialNumber,
            deviceUuid,
            cts.Token);

        if (string.IsNullOrWhiteSpace(paired.DeviceToken))
            throw new InvalidOperationException("Handshake did not return a device token.");

        state = new SimulatorState
        {
            SerialNumber = paired.SerialNumber,
            DeviceId = paired.DeviceId,
            HiveId = paired.HiveId,
            DeviceUuid = deviceUuid,
            DeviceToken = paired.DeviceToken
        };

        SaveState(statePath, state);

        Console.WriteLine($"[HANDSHAKE] Device paired. DeviceId={paired.DeviceId}");
    }
    else
    {
        Console.WriteLine($"[STATE] Loaded existing token for DeviceId={state.DeviceId}");
    }

    Console.WriteLine("[SIM] Sending telemetry. Press Ctrl+C to stop.");

    while (!cts.Token.IsCancellationRequested)
    {
        TelemetryRequest telemetry = telemetryGenerator.Generate(deviceUuid);

        await telemetryPublisher.PublishAsync(
            telemetry,
            state.DeviceToken,
            cts.Token);

        Console.WriteLine(
            $"[{telemetry.MeasuredAt:HH:mm:ss}] " +
            $"weight={telemetry.WeightKg}kg, " +
            $"temp={telemetry.TemperatureC}C, " +
            $"humidity={telemetry.HumidityPercent}%, " +
            $"battery={telemetry.BatteryPercent}%");

        await Task.Delay(TimeSpan.FromSeconds(settings.DelaySeconds), cts.Token);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("[SIM] Stopped.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"[ERROR] Cannot reach Functions host: {ex.Message}");
    Console.WriteLine("Start Azurite and run: func start --port 7071");
    Environment.ExitCode = 1;
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
    Environment.ExitCode = 1;
}

static async Task<RegisterDeviceResponse> RegisterOrContinueAsync(
    DeviceClient deviceClient,
    string serialNumber,
    Guid hiveId,
    CancellationToken ct)
{
    try
    {
        return await deviceClient.RegisterAsync(serialNumber, hiveId, ct);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("409"))
    {
        Console.WriteLine("[REGISTER] Device already exists. Continuing with handshake.");
        return new RegisterDeviceResponse
        {
            SerialNumber = serialNumber,
            HiveId = hiveId,
            Status = "AlreadyRegistered"
        };
    }
}

static void ValidateSettings(SimulatorSettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.FunctionsBaseUrl))
        throw new InvalidOperationException("FunctionsBaseUrl is required.");

    if (string.IsNullOrWhiteSpace(settings.SerialNumber))
        throw new InvalidOperationException("SerialNumber is required.");

    if (settings.DelaySeconds < 1)
        throw new InvalidOperationException("DelaySeconds must be at least 1.");
}

static string NormalizeBaseUrl(string baseUrl) =>
    baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";

static SimulatorState? LoadState(string statePath, string serialNumber)
{
    if (!File.Exists(statePath))
        return null;

    string json = File.ReadAllText(statePath);
    SimulatorState? state = JsonSerializer.Deserialize<SimulatorState>(json);

    if (state?.SerialNumber != serialNumber || string.IsNullOrWhiteSpace(state.DeviceToken))
        return null;

    return state;
}

static void SaveState(string statePath, SimulatorState state)
{
    string json = JsonSerializer.Serialize(state, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText(statePath, json);
}

static void PrintHeader(SimulatorSettings settings, Guid hiveId, Guid deviceUuid)
{
    Console.WriteLine("Smart Apiary Scale Simulator");
    Console.WriteLine($"Functions: {NormalizeBaseUrl(settings.FunctionsBaseUrl)}");
    Console.WriteLine($"Serial:    {settings.SerialNumber}");
    Console.WriteLine($"HiveId:    {hiveId}");
    Console.WriteLine($"UUID:      {deviceUuid}");
    Console.WriteLine();
}
