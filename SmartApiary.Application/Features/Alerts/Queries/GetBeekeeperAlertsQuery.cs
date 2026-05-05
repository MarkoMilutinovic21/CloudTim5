namespace SmartApiary.Application.Features.Alerts.Queries;

using MediatR;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Domain.Models;

public record BeekeeperAlertDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    DateTime CreatedAt);

public record GetBeekeeperAlertsQuery(Guid BeekeeperId)
    : IRequest<IReadOnlyCollection<BeekeeperAlertDto>>;

public class GetBeekeeperAlertsQueryHandler(
    IBeekeeperAlertRepository alertRepository)
    : IRequestHandler<GetBeekeeperAlertsQuery, IReadOnlyCollection<BeekeeperAlertDto>>
{
    public async Task<IReadOnlyCollection<BeekeeperAlertDto>> Handle(
        GetBeekeeperAlertsQuery request,
        CancellationToken ct)
    {
        IReadOnlyCollection<BeekeeperAlert> alerts =
            await alertRepository.GetByBeekeeperIdAsync(request.BeekeeperId, ct);

        return alerts
            .OrderByDescending(alert => alert.CreatedAt)
            .Select(alert => new BeekeeperAlertDto(
                alert.Id,
                alert.Type,
                alert.Title,
                alert.Message,
                alert.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
