namespace SmartApiary.Application.Features.PesticideTreatments.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.PesticideTreatments;
using SmartApiary.Application.Features.Alerts;
using SmartApiary.Domain.Models;

public record CancelPesticideTreatmentCommand(
    Guid TreatmentId,
    Guid FarmerId) : IRequest;

public class CancelPesticideTreatmentCommandHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository,
    IApiaryRepository apiaryRepository,
    IUserRepository userRepository,
    IBeekeeperAlertRepository alertRepository,
    IEmailService emailService)
    : IRequestHandler<CancelPesticideTreatmentCommand>
{
    public async Task Handle(CancelPesticideTreatmentCommand request, CancellationToken ct)
    {
        PesticideTreatment? treatment =
            await treatmentRepository.GetByIdAsync(request.TreatmentId, ct);

        if (treatment is null)
        {
            throw new KeyNotFoundException("Najava tretiranja nije pronađena.");
        }

        if (treatment.FarmerId != request.FarmerId)
        {
            throw new UnauthorizedAccessException("Nemate pristup ovoj najavi.");
        }

        Parcel? parcel = await parcelRepository.GetByIdAsync(treatment.ParcelId, ct);

        if (parcel is null)
        {
            throw new KeyNotFoundException("Parcela nije pronađena.");
        }

        IReadOnlyCollection<User> nearbyBeekeepers =
            await PesticideTreatmentNotificationHelper.GetNearbyBeekeepersAsync(
                parcel,
                apiaryRepository,
                userRepository,
                ct);

        string message = PesticideTreatmentNotificationHelper.CreateCancelledMessage(parcel);

        await BeekeeperAlertHelper.CreateAlertsAsync(
            nearbyBeekeepers,
            alertRepository,
            emailService,
            BeekeeperAlertTypes.PesticideTreatment,
            "Otkazivanje najave tretiranja pesticidima - Smart Apiary",
            message,
            ct);

        treatment.Cancel();

        await treatmentRepository.UpdateAsync(treatment, ct);
    }
}
