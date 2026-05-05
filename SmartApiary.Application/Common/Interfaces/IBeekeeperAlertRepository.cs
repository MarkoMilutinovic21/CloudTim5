namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IBeekeeperAlertRepository
{
    Task<IReadOnlyCollection<BeekeeperAlert>> GetByBeekeeperIdAsync(
        Guid beekeeperId,
        CancellationToken ct = default);

    Task SaveAsync(BeekeeperAlert alert, CancellationToken ct = default);
}
