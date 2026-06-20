namespace SmartApiary.Application.Features.Devices.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record RegisterDeviceForHiveCommand(
    Guid ApiaryId,
    Guid HiveId,
    string SerialNumber,
    Guid OwnerId) : IRequest<RegisterDeviceForHiveResult>;

public record RegisterDeviceForHiveResult(
    Guid DeviceId,
    Guid HiveId,
    string SerialNumber,
    string Status);

public class RegisterDeviceForHiveCommandValidator : AbstractValidator<RegisterDeviceForHiveCommand>
{
    public RegisterDeviceForHiveCommandValidator()
    {
        RuleFor(x => x.ApiaryId).NotEmpty();
        RuleFor(x => x.HiveId).NotEmpty();
        RuleFor(x => x.SerialNumber).NotEmpty().Matches("^SA-[0-9]{4}-[0-9]{5}$");
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class RegisterDeviceForHiveCommandHandler(
    IApiaryRepository apiaryRepository,
    IHiveRepository hiveRepository,
    IDeviceRepository deviceRepository) : IRequestHandler<RegisterDeviceForHiveCommand, RegisterDeviceForHiveResult>
{
    public async Task<RegisterDeviceForHiveResult> Handle(RegisterDeviceForHiveCommand request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);

        if (apiary is null)
            throw new Exception("Pčelinjak nije pronađen.");

        if (apiary.OwnerId != request.OwnerId)
            throw new Exception("Nemate pristup ovom pčelinjaku.");

        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);

        if (hive is null)
            throw new Exception("Košnica nije pronađena.");

        if (hive.ApiaryId != request.ApiaryId)
            throw new Exception("Košnica ne pripada ovom pčelinjaku.");

        var existingBySerial = await deviceRepository.GetBySerialNumberAsync(request.SerialNumber, ct);
        if (existingBySerial is not null)
            throw new Exception("Uređaj sa ovim serijskim brojem je već registrovan.");

        var existingByHive = await deviceRepository.GetByHiveIdAsync(request.HiveId, ct);
        if (existingByHive is not null)
            throw new Exception("Za ovu košnicu je uređaj već registrovan.");

        Device device = Device.Create(request.SerialNumber, request.HiveId);
        await deviceRepository.SaveAsync(device, ct);

        return new RegisterDeviceForHiveResult(
            device.Id,
            device.HiveId,
            device.SerialNumber,
            device.Status);
    }
}
