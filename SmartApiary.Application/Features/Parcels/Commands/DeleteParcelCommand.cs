namespace SmartApiary.Application.Features.Parcels.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteParcelCommand(Guid ParcelId, Guid OwnerId) : IRequest;

public class DeleteParcelCommandHandler(
    IParcelRepository parcelRepository) : IRequestHandler<DeleteParcelCommand>
{
    public async Task Handle(DeleteParcelCommand request, CancellationToken ct)
    {
        var parcel = await parcelRepository.GetByIdAsync(request.ParcelId, ct);

        if (parcel is null)
        {
            throw new KeyNotFoundException("Parcela nije pronađena.");
        }

        if (parcel.OwnerId != request.OwnerId)
        {
            throw new UnauthorizedAccessException("Nemate pristup ovoj parceli.");
        }

        await parcelRepository.DeleteAsync(parcel, ct);
    }
}