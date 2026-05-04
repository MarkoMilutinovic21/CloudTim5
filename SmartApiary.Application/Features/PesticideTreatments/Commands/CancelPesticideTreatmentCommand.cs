namespace SmartApiary.Application.Features.PesticideTreatments.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.PesticideTreatments;
using SmartApiary.Domain.Models;

public record CancelPesticideTreatmentCommand(
    Guid TreatmentId,
    Guid FarmerId) : IRequest;

public class CancelPesticideTreatmentCommandHandler(
    IPesticideTreatmentRepository treatmentRepository,
    IParcelRepository parcelRepository,
    IApiaryRepository apiaryRepository,
    IUserRepository userRepository,
    IEmailService emailService)
    : IRequestHandler<CancelPesticideTreatmentCommand>
{
    public async Task Handle(CancelPesticideTreatmentCommand request, CancellationToken ct)
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

        Parcel? parcel = await parcelRepository.GetByIdAsync(treatment.ParcelId, ct);

        if (parcel is null)
        {
            throw new Exception("Parcela nije pronađena.");
        }

        IReadOnlyCollection<User> nearbyBeekeepers =
            await PesticideTreatmentNotificationHelper.GetNearbyBeekeepersAsync(
                parcel,
                apiaryRepository,
                userRepository,
                ct);

        string message = PesticideTreatmentNotificationHelper.CreateCancelledMessage(parcel);

        await PesticideTreatmentNotificationHelper.TryNotifyBeekeepersAsync(
            nearbyBeekeepers,
            emailService,
            "Otkazivanje najave tretiranja pesticidima - Smart Apiary",
            message,
            ct);

        treatment.Cancel();

        await treatmentRepository.UpdateAsync(treatment, ct);
    }
}