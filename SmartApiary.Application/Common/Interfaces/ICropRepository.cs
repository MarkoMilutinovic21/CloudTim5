using SmartApiary.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Common.Interfaces
{
    public interface ICropRepository
    {
        Task SaveAsync(Crop crop, CancellationToken ct = default);
        Task<IReadOnlyCollection<Crop>> GetByParcelIdAsync(Guid parcelId, CancellationToken ct = default);
    }
}
