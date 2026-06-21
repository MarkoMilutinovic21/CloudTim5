namespace SmartApiary.Application.Features.Apiaries.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteApiaryCommand(Guid ApiaryId, Guid OwnerId) : IRequest;

public class DeleteApiaryCommandHandler(
    IApiaryRepository apiaryRepository) : IRequestHandler<DeleteApiaryCommand>
{
    public async Task Handle(DeleteApiaryCommand request, CancellationToken ct)
    {
        var apiary = await apiaryRepository.GetByIdAsync(request.ApiaryId, ct);
        if (apiary is null) throw new KeyNotFoundException("Pčelinjak nije pronađen.");
        if (apiary.OwnerId != request.OwnerId) throw new UnauthorizedAccessException("Nemate pristup ovom pčelinjaku.");
        await apiaryRepository.DeleteAsync(apiary, ct);
    }
}