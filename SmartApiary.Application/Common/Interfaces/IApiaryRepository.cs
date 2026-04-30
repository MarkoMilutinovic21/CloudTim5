using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IApiaryRepository
{
    Task<Apiary?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Apiary>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task SaveAsync(Apiary apiary, CancellationToken ct = default);
    Task UpdateAsync(Apiary apiary, CancellationToken ct = default);
    Task DeleteAsync(Apiary apiary, CancellationToken ct = default);
}