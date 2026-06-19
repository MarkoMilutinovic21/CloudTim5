namespace SmartApiary.Application.Features.SprayingRecords.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record CreateSprayingRecordCommand(
    DateTime StartTime,
    double DurationHours,
    string ChemicalName,
    Guid ParcelId
) : IRequest<CreateSprayingRecordResult>;

public record CreateSprayingRecordResult(Guid Id, string? WindWarning);

public class CreateSprayingRecordValidator : AbstractValidator<CreateSprayingRecordCommand>
{
    public CreateSprayingRecordValidator()
    {
        RuleFor(x => x.ParcelId).NotEmpty();
        RuleFor(x => x.DurationHours).GreaterThan(0);
        RuleFor(x => x.ChemicalName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Datum prskanja ne može biti u budućnosti.");
    }
}

public class CreateSprayingRecordHandler(
    ISprayingRecordRepository repo,
    IParcelRepository parcelRepository,
    IWeatherService weatherService
) : IRequestHandler<CreateSprayingRecordCommand, CreateSprayingRecordResult>
{
    private const double WindSpeedThresholdMs = 5.0;

    public async Task<CreateSprayingRecordResult> Handle(CreateSprayingRecordCommand request, CancellationToken ct)
    {
        var record = SprayingRecord.Create(
            request.StartTime,
            request.DurationHours,
            request.ChemicalName,
            request.ParcelId
        );

        await repo.SaveAsync(record, ct);

        string? windWarning = null;
        var parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is not null)
        {
            var windSpeed = await weatherService.GetCurrentWindSpeedAsync(parcel.Latitude, parcel.Longitude, ct);
            if (windSpeed.HasValue && windSpeed.Value > WindSpeedThresholdMs)
            {
                windWarning = $"Brzina vjetra na parceli \"{parcel.Name}\" u trenutku prskanja bila je {windSpeed.Value:F1} m/s — uslovi nisu bili idealni za prskanje.";
            }
        }

        return new CreateSprayingRecordResult(record.Id, windWarning);
    }
}
