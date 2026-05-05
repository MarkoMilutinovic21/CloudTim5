namespace SmartApiary.Application.Features.PesticideTreatments.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.Alerts;
using SmartApiary.Application.Features.PesticideTreatments;
using SmartApiary.Domain.Models;

public record UpdatePesticideTreatmentCommand(
    Guid TreatmentId,
    Guid ParcelId,
    DateTime PlannedStartAt,
    double DurationHours,
    string? PesticideType,
    Guid FarmerId) : IRequest<UpdatePesticideTreatmentResult>;

public record UpdatePesticideTreatmentResult(
    int NotifiedBeekeepersCount);

public class UpdatePesticideTreatmentCommandValidator : AbstractValidator<UpdatePesticideTreatmentCommand>
{
    public UpdatePesticideTreatmentCommandValidator()
    {
        RuleFor(x => x.TreatmentId)
            .NotEmpty();

        RuleFor(x => x.ParcelId)
            .NotEmpty();

        RuleFor(x => x.PlannedStartAt)
            .GreaterThan(DateTime.MinValue);

        RuleFor(x => x.DurationHours)
            .GreaterThan(0)
            .LessThanOrEqualTo(24);

        RuleFor(x => x.PesticideType)
            .MaximumLength(200);

        RuleFor(x => x.FarmerId)
            .NotEmpty();
    }
}

public class UpdatePesticideTreatmentCommandHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository,
    IApiaryRepository apiaryRepository,
    IUserRepository userRepository,
    IBeekeeperAlertRepository alertRepository,
    IEmailService emailService)
    : IRequestHandler<UpdatePesticideTreatmentCommand, UpdatePesticideTreatmentResult>
{
    public async Task<UpdatePesticideTreatmentResult> Handle(
        UpdatePesticideTreatmentCommand request,
        CancellationToken ct)
    {
        PesticideTreatment? treatment =
            await treatmentRepository.GetByIdAsync(request.TreatmentId, ct);

        if (treatment is null)
        {
            throw new Exception("Najava tretiranja nije pronađena.");
        }

        if (treatment.FarmerId != request.FarmerId)
        {
            throw new Exception("Nemate pristup ovoj najavi.");
        }

        Parcel? parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);

        if (parcel is null)
        {
            throw new Exception("Parcela nije pronađena.");
        }

        if (parcel.OwnerId != request.FarmerId)
        {
            throw new Exception("Nemate pristup ovoj parceli.");
        }

        IReadOnlyCollection<User> nearbyBeekeepers =
            await PesticideTreatmentNotificationHelper.GetNearbyBeekeepersAsync(
                parcel,
                apiaryRepository,
                userRepository,
                ct);

        string pesticideType = request.PesticideType ?? string.Empty;

        string message = PesticideTreatmentNotificationHelper.CreateUpdatedMessage(
            parcel,
            request.PlannedStartAt,
            request.DurationHours,
            pesticideType);

        int notifiedCount = await BeekeeperAlertHelper.CreateAlertsAsync(
            nearbyBeekeepers,
            alertRepository,
            emailService,
            BeekeeperAlertTypes.PesticideTreatment,
            "Izmena najave tretiranja pesticidima - Smart Apiary",
            message,
            ct);

        treatment.Update(
            request.ParcelId,
            request.PlannedStartAt,
            request.DurationHours,
            pesticideType,
            notifiedCount);

        await treatmentRepository.UpdateAsync(treatment, ct);

        return new UpdatePesticideTreatmentResult(notifiedCount);
    }
}
