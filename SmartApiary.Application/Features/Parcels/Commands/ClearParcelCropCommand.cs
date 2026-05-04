namespace SmartApiary.Application.Features.Parcels.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record ClearParcelCropCommand(Guid ParcelId, Guid OwnerId) : IRequest;

public class ClearParcelCropCommandHandler(
    IParcelRepository parcelRepository) : IRequestHandler<ClearParcelCropCommand>
{
    public async Task Handle(ClearParcelCropCommand request, CancellationToken ct)
    {
        var parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);
        if (parcel is null) throw new Exception("Parcela nije pronađena.");
        if (parcel.OwnerId != request.OwnerId) throw new Exception("Nemate pristup ovoj parceli.");

        parcel.ClearCrop();

        await parcelRepository.UpdateAsync(parcel, ct);
    }
}
