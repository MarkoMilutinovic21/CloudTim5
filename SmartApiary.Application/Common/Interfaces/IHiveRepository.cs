using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IHiveRepository
{
    Task<Hive?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Hive>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<Hive>> GetByApiaryIdAsync(Guid apiaryId, CancellationToken ct = default);
    Task SaveAsync(Hive hive, CancellationToken ct = default);
    Task UpdateAsync(Hive hive, CancellationToken ct = default);
    Task DeleteAsync(Hive hive, CancellationToken ct = default);
}
