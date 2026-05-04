namespace SmartApiary.Simulator.Services;

using SmartApiary.Simulator.Configuration;
using SmartApiary.Simulator.Models;

public class TelemetryGenerator(SimulatorSettings settings)
{
    private readonly Random _random = new();
    private double _currentWeightKg = settings.InitialWeightKg;
    private double _currentBatteryPercent = settings.InitialBatteryPercent;

    public TelemetryRequest Generate(Guid deviceUuid)
    {
        _currentWeightKg += NextDelta(settings.WeightVariationKg);
        _currentBatteryPercent = Math.Max(0, _currentBatteryPercent - _random.NextDouble() * 0.08);

        return new TelemetryRequest
        {
            DeviceUuid = deviceUuid,
            WeightKg = Math.Round(_currentWeightKg, 2),
            TemperatureC = Math.Round(32 + NextDelta(2.5), 2),
            HumidityPercent = Math.Round(60 + NextDelta(8), 2),
            BatteryPercent = Math.Round(_currentBatteryPercent, 2),
            MeasuredAt = DateTime.UtcNow
        };
    }

    private double NextDelta(double range) =>
        (_random.NextDouble() * 2 - 1) * range;
}
