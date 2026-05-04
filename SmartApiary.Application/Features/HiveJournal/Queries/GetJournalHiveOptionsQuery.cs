namespace SmartApiary.Application.Features.HiveJournal.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;

public record GetJournalHiveOptionsQuery : IRequest<IReadOnlyCollection<JournalHiveOptionDto>>;

public record JournalHiveOptionDto(
    Guid HiveId,
    string Label,
    string Source);

public class GetJournalHiveOptionsQueryHandler(
    IHiveRepository hiveRepository,
    IDeviceRepository deviceRepository) : IRequestHandler<GetJournalHiveOptionsQuery, IReadOnlyCollection<JournalHiveOptionDto>>
{
    public async Task<IReadOnlyCollection<JournalHiveOptionDto>> Handle(GetJournalHiveOptionsQuery request, CancellationToken ct)
    {
        var hives = await hiveRepository.GetAllAsync(ct);
        var devices = await deviceRepository.GetAllAsync(ct);

        var options = hives
            .Select(hive => new JournalHiveOptionDto(
                hive.Id,
                string.IsNullOrWhiteSpace(hive.Name) ? hive.Id.ToString() : $"{hive.Name} ({hive.Id})",
                "Hive"))
            .Concat(devices.Select(device => new JournalHiveOptionDto(
                device.HiveId,
                string.IsNullOrWhiteSpace(device.SerialNumber)
                    ? device.HiveId.ToString()
                    : $"{device.SerialNumber} ({device.HiveId})",
                "Simulator")))
            .GroupBy(option => option.HiveId)
            .Select(group => group.First())
            .OrderBy(option => option.Source)
            .ThenBy(option => option.Label)
            .ToList()
            .AsReadOnly();

        return options;
    }
}
