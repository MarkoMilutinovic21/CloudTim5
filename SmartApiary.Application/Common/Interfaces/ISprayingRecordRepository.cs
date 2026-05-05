using SmartApiary.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Common.Interfaces
{
    public interface ISprayingRecordRepository
    {
        Task SaveAsync(SprayingRecord record, CancellationToken ct = default);
        Task<IReadOnlyCollection<SprayingRecord>> GetByParcelIdAsync(Guid parcelId, CancellationToken ct = default);
    }
}
