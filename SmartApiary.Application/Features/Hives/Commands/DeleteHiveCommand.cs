using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Features.Hives.Commands;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record DeleteHiveCommand(Guid HiveId, Guid ApiaryId) : IRequest;

public class DeleteHiveCommandHandler(
    IHiveRepository hiveRepository) : IRequestHandler<DeleteHiveCommand>
{
    public async Task Handle(DeleteHiveCommand request, CancellationToken ct)
    {
        var hive = await hiveRepository.GetByIdAsync(request.HiveId, ct);
        if (hive is null) throw new Exception("Košnica nije pronađena.");
        if (hive.ApiaryId != request.ApiaryId) throw new Exception("Košnica ne pripada ovom pčelinjaku.");
        await hiveRepository.DeleteAsync(hive, ct);
    }
}