using NSubstitute;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.Telemetry.Queries;
using SmartApiary.Domain.Models;
using Xunit;

namespace SmartApiary.Tests;

public sealed class TelemetryAuthorizationTests
{
    [Fact]
    public async Task ForeignBeekeeper_CannotReadLatestHiveTelemetry()
    {
        Guid ownerId = Guid.NewGuid();
        Guid foreignBeekeeperId = Guid.NewGuid();
        Apiary apiary = Apiary.Create(
            "Pčelinjak", "", "Novi Sad", 45.25, 19.84, "", "", ownerId);
        Hive hive = Hive.Create("K1", "LR", "plava", 1, "", apiary.Id);

        var measurements = Substitute.For<ITelemetryMeasurementRepository>();
        var hives = Substitute.For<IHiveRepository>();
        var apiaries = Substitute.For<IApiaryRepository>();
        hives.GetByIdAsync(hive.Id, Arg.Any<CancellationToken>()).Returns(hive);
        apiaries.GetByIdAsync(apiary.Id, Arg.Any<CancellationToken>()).Returns(apiary);
        var handler = new GetLatestHiveTelemetryQueryHandler(measurements, hives, apiaries);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetLatestHiveTelemetryQuery(hive.Id, foreignBeekeeperId),
                CancellationToken.None));

        await measurements.DidNotReceive()
            .GetLatestForHiveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Owner_CanReadLatestHiveTelemetry()
    {
        Guid ownerId = Guid.NewGuid();
        Apiary apiary = Apiary.Create(
            "Pčelinjak", "", "Novi Sad", 45.25, 19.84, "", "", ownerId);
        Hive hive = Hive.Create("K1", "LR", "plava", 1, "", apiary.Id);
        TelemetryMeasurement measurement = TelemetryMeasurement.Create(
            Guid.NewGuid(), hive.Id, Guid.NewGuid(), 42, 32, 60, 90, DateTime.UtcNow);

        var measurements = Substitute.For<ITelemetryMeasurementRepository>();
        var hives = Substitute.For<IHiveRepository>();
        var apiaries = Substitute.For<IApiaryRepository>();
        hives.GetByIdAsync(hive.Id, Arg.Any<CancellationToken>()).Returns(hive);
        apiaries.GetByIdAsync(apiary.Id, Arg.Any<CancellationToken>()).Returns(apiary);
        measurements.GetLatestForHiveAsync(hive.Id, Arg.Any<CancellationToken>()).Returns(measurement);
        var handler = new GetLatestHiveTelemetryQueryHandler(measurements, hives, apiaries);

        TelemetryMeasurementDto? result = await handler.Handle(
            new GetLatestHiveTelemetryQuery(hive.Id, ownerId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(42, result.WeightKg);
    }
}
