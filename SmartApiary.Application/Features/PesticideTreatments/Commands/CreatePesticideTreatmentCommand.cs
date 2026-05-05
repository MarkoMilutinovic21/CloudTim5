namespace SmartApiary.Application.Features.PesticideTreatments.Commands;

using FluentValidation;
using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.Alerts;
using SmartApiary.Application.Features.PesticideTreatments;
using SmartApiary.Domain.Models;

public record CreatePesticideTreatmentCommand(
    Guid ParcelId,
    DateTime PlannedStartAt,
    double DurationHours,
    string? PesticideType,
    Guid FarmerId) : IRequest<CreatePesticideTreatmentResult>;

public record CreatePesticideTreatmentResult(
    Guid Id,
    int NotifiedBeekeepersCount);

public class CreatePesticideTreatmentCommandValidator : AbstractValidator<CreatePesticideTreatmentCommand>
{
    public CreatePesticideTreatmentCommandValidator()
    {
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

public class CreatePesticideTreatmentCommandHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository,
    IApiaryRepository apiaryRepository,
    IUserRepository userRepository,
    IBeekeeperAlertRepository alertRepository,
    IEmailService emailService)
    : IRequestHandler<CreatePesticideTreatmentCommand, CreatePesticideTreatmentResult>
{
    public async Task<CreatePesticideTreatmentResult> Handle(
        CreatePesticideTreatmentCommand request,
        CancellationToken ct)
    {
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

        string message = PesticideTreatmentNotificationHelper.CreateScheduledMessage(
            parcel,
            request.PlannedStartAt,
            request.DurationHours,
            pesticideType);

        int notifiedCount = await BeekeeperAlertHelper.CreateAlertsAsync(
            nearbyBeekeepers,
            alertRepository,
            emailService,
            BeekeeperAlertTypes.PesticideTreatment,
            "Upozorenje o tretiranju pesticidima - Smart Apiary",
            message,
            ct);

        PesticideTreatment treatment = PesticideTreatment.Create(
            request.ParcelId,
            request.FarmerId,
            request.PlannedStartAt,
            request.DurationHours,
            pesticideType,
            notifiedCount);

        await treatmentRepository.SaveAsync(treatment, ct);

        return new CreatePesticideTreatmentResult(
            treatment.Id,
            treatment.NotifiedBeekeepersCount);
    }
}
