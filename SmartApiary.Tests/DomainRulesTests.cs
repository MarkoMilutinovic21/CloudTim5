using SmartApiary.Domain.Common;
using SmartApiary.Domain.Models;
using Xunit;

namespace SmartApiary.Tests;

public sealed class DomainRulesTests
{
    [Fact]
    public void NewBeekeeper_UsesTenKilogramDefaultThreshold()
    {
        User user = User.Create(
            "Pera", "Perić", "pera@example.com", "hash", UserRoles.Beekeeper);

        Assert.Equal(10, user.WeightDropThresholdKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void WeightThreshold_RejectsValuesOutsideSupportedRange(double threshold)
    {
        User user = User.Create(
            "Pera", "Perić", "pera@example.com", "hash", UserRoles.Beekeeper);

        Assert.Throws<ArgumentOutOfRangeException>(() => user.SetWeightDropThreshold(threshold));
    }

    [Fact]
    public void RegisteredDevice_RemainsUnpairedUntilHandshake()
    {
        Device device = Device.Create("SA-2026-12345", Guid.NewGuid());

        Assert.Equal(DevicePairingStatus.Unpaired, device.Status);
        Assert.Null(device.DeviceUuid);
        Assert.Null(device.DeviceToken);
    }

    [Fact]
    public void Handshake_PairsDeviceAndAcceptsOnlyGeneratedToken()
    {
        Device device = Device.Create("SA-2026-12345", Guid.NewGuid());
        Guid deviceUuid = Guid.NewGuid();

        device.Pair(deviceUuid, "secure-device-token");

        Assert.Equal(DevicePairingStatus.Paired, device.Status);
        Assert.Equal(deviceUuid, device.DeviceUuid);
        Assert.True(device.IsTokenValid("secure-device-token"));
        Assert.False(device.IsTokenValid("wrong-token"));
    }

    [Fact]
    public void CancelledTreatment_CannotBeCompleted()
    {
        PesticideTreatment treatment = PesticideTreatment.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2, "Preparat", 3);

        treatment.Cancel();
        treatment.Complete();

        Assert.Equal(PesticideTreatmentStatuses.Cancelled, treatment.Status);
    }

    [Fact]
    public void CompletedTreatment_CreatesFullSprayingRecord()
    {
        Guid farmerId = Guid.NewGuid();
        Parcel parcel = Parcel.Create(
            "Njiva 1", 3.5, "Novi Sad", 45.25, 19.84, "", farmerId);
        parcel.SetCrop("Suncokret", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddMonths(1), "");
        PesticideTreatment treatment = PesticideTreatment.Create(
            parcel.Id, farmerId, DateTime.UtcNow.AddHours(-3), 2, "Preparat X", 2);

        SprayingRecord record = SprayingRecord.CreateFromTreatment(
            treatment, parcel, "vedro", 2.4, false);

        Assert.Equal(treatment.Id, record.TreatmentId);
        Assert.Equal("Suncokret", record.CropName);
        Assert.Equal("Njiva 1", record.ParcelName);
        Assert.Equal(treatment.PlannedStartAt.AddHours(2), record.EndTime);
        Assert.Equal(2.4, record.WindSpeedMs);
    }
}
