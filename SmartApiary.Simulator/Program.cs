using Azure;
using Azure.Data.Tables;
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
TableClient devicesTable = new(settings.StorageConnectionString, settings.DevicesTable);

string statesPath = Path.Combine(AppContext.BaseDirectory, "device-states.json");
string legacyStatePath = Path.Combine(AppContext.BaseDirectory, "device-state.json");
Dictionary<string, SimulatorState> states = LoadStates(statesPath, legacyStatePath);
Dictionary<string, TelemetryGenerator> generators = states.Keys.ToDictionary(
    serialNumber => serialNumber,
    _ => new TelemetryGenerator(settings),
    StringComparer.OrdinalIgnoreCase);

Console.WriteLine("Smart Apiary Multi-device Simulator");
Console.WriteLine($"Functions: {NormalizeBaseUrl(settings.FunctionsBaseUrl)}");
Console.WriteLine($"Devices:   {settings.DevicesTable} (automatic discovery)");
Console.WriteLine($"Known:     {states.Count} paired device(s)");
Console.WriteLine("[SIM] Discovering devices and sending telemetry. Press Ctrl+C to stop.");

while (!cts.Token.IsCancellationRequested)
{
    try
    {
        IReadOnlyCollection<RegisteredDevice> registeredDevices =
            await GetRegisteredDevicesAsync(devicesTable, cts.Token);

        foreach (RegisteredDevice device in registeredDevices
                     .Where(device => device.Status.Equals("Paired", StringComparison.OrdinalIgnoreCase)))
        {
            if (states.ContainsKey(device.SerialNumber) ||
                !device.DeviceId.HasValue ||
                !device.HiveId.HasValue ||
                !device.DeviceUuid.HasValue ||
                string.IsNullOrWhiteSpace(device.DeviceToken))
            {
                continue;
            }

            SimulatorState recoveredState = new()
            {
                SerialNumber = device.SerialNumber,
                DeviceId = device.DeviceId.Value,
                HiveId = device.HiveId.Value,
                DeviceUuid = device.DeviceUuid.Value,
                DeviceToken = device.DeviceToken
            };

            states[recoveredState.SerialNumber] = recoveredState;
            generators[recoveredState.SerialNumber] = new TelemetryGenerator(settings);
            SaveStates(statesPath, states);

            Console.WriteLine($"[RECOVER] {recoveredState.SerialNumber} -> hive {recoveredState.HiveId}");
        }

        foreach (RegisteredDevice device in registeredDevices
                     .Where(device => device.Status.Equals("Unpaired", StringComparison.OrdinalIgnoreCase)))
        {
            if (states.ContainsKey(device.SerialNumber))
                continue;

            try
            {
                Guid deviceUuid = Guid.NewGuid();
                RegisterDeviceResponse paired = await deviceClient.HandshakeAsync(
                    device.SerialNumber,
                    deviceUuid,
                    cts.Token);

                if (string.IsNullOrWhiteSpace(paired.DeviceToken))
                    throw new InvalidOperationException("Handshake did not return a device token.");

                SimulatorState state = new()
                {
                    SerialNumber = paired.SerialNumber,
                    DeviceId = paired.DeviceId,
                    HiveId = paired.HiveId,
                    DeviceUuid = deviceUuid,
                    DeviceToken = paired.DeviceToken
                };

                states[state.SerialNumber] = state;
                generators[state.SerialNumber] = new TelemetryGenerator(settings);
                SaveStates(statesPath, states);

                Console.WriteLine(
                    $"[PAIR] {state.SerialNumber} -> hive {state.HiveId}, device {state.DeviceId}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"[PAIR ERROR] {device.SerialNumber}: {ex.Message}");
            }
        }

        HashSet<string> existingSerialNumbers = registeredDevices
            .Select(device => device.SerialNumber)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (SimulatorState state in states.Values
                     .Where(state => existingSerialNumbers.Contains(state.SerialNumber))
                     .ToList())
        {
            try
            {
                TelemetryGenerator generator = generators[state.SerialNumber];
                TelemetryRequest telemetry = generator.Generate(state.DeviceUuid);

                await telemetryPublisher.PublishAsync(
                    telemetry,
                    state.DeviceToken,
                    cts.Token);

                Console.WriteLine(
                    $"[{telemetry.MeasuredAt:HH:mm:ss}] {state.SerialNumber} " +
                    $"hive={state.HiveId} weight={telemetry.WeightKg}kg, " +
                    $"temp={telemetry.TemperatureC}C, humidity={telemetry.HumidityPercent}%, " +
                    $"battery={telemetry.BatteryPercent}%");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"[SEND ERROR] {state.SerialNumber}: {ex.Message}");
            }
        }
    }
    catch (RequestFailedException ex)
    {
        Console.WriteLine($"[DISCOVERY ERROR] Cannot read devices table: {ex.Message}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[CONNECTION ERROR] Cannot reach Functions host: {ex.Message}");
    }

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(settings.DelaySeconds), cts.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}

Console.WriteLine("[SIM] Stopped.");

static async Task<IReadOnlyCollection<RegisteredDevice>> GetRegisteredDevicesAsync(
    TableClient tableClient,
    CancellationToken ct)
{
    List<RegisteredDevice> devices = new();

    await foreach (TableEntity entity in tableClient.QueryAsync<TableEntity>(
                       maxPerPage: 100,
                       cancellationToken: ct))
    {
        string? serialNumber = entity.GetString("SerialNumber");
        string? status = entity.GetString("Status");

        if (string.IsNullOrWhiteSpace(serialNumber) || string.IsNullOrWhiteSpace(status))
            continue;

        devices.Add(new RegisteredDevice(
            serialNumber,
            status,
            ParseGuid(entity.RowKey),
            ParseGuid(entity.GetString("HiveId")),
            ParseGuid(entity.GetString("DeviceUuid")),
            entity.GetString("DeviceToken")));
    }

    return devices;
}

static Dictionary<string, SimulatorState> LoadStates(string statesPath, string legacyStatePath)
{
    Dictionary<string, SimulatorState> states = new(StringComparer.OrdinalIgnoreCase);

    if (File.Exists(statesPath))
    {
        List<SimulatorState>? saved = JsonSerializer.Deserialize<List<SimulatorState>>(
            File.ReadAllText(statesPath));

        foreach (SimulatorState state in saved ?? [])
        {
            if (IsValidState(state))
                states[state.SerialNumber] = state;
        }
    }

    if (File.Exists(legacyStatePath))
    {
        SimulatorState? legacy = JsonSerializer.Deserialize<SimulatorState>(
            File.ReadAllText(legacyStatePath));

        if (legacy is not null && IsValidState(legacy))
            states.TryAdd(legacy.SerialNumber, legacy);
    }

    return states;
}

static void SaveStates(string statesPath, Dictionary<string, SimulatorState> states)
{
    File.WriteAllText(
        statesPath,
        JsonSerializer.Serialize(
            states.Values.OrderBy(state => state.SerialNumber),
            new JsonSerializerOptions { WriteIndented = true }));
}

static bool IsValidState(SimulatorState state) =>
    !string.IsNullOrWhiteSpace(state.SerialNumber) &&
    state.DeviceId != Guid.Empty &&
    state.HiveId != Guid.Empty &&
    state.DeviceUuid != Guid.Empty &&
    !string.IsNullOrWhiteSpace(state.DeviceToken);

static void ValidateSettings(SimulatorSettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.FunctionsBaseUrl))
        throw new InvalidOperationException("FunctionsBaseUrl is required.");

    if (string.IsNullOrWhiteSpace(settings.StorageConnectionString))
        throw new InvalidOperationException("StorageConnectionString is required.");

    if (string.IsNullOrWhiteSpace(settings.DevicesTable))
        throw new InvalidOperationException("DevicesTable is required.");

    if (settings.DelaySeconds < 1)
        throw new InvalidOperationException("DelaySeconds must be at least 1.");
}

static string NormalizeBaseUrl(string baseUrl) =>
    baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";

static Guid? ParseGuid(string? value) =>
    Guid.TryParse(value, out Guid parsed) ? parsed : null;

internal sealed record RegisteredDevice(
    string SerialNumber,
    string Status,
    Guid? DeviceId,
    Guid? HiveId,
    Guid? DeviceUuid,
    string? DeviceToken);
