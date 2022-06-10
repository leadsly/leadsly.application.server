using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ITimeZoneRepository
    {
        Task<HalTimeZone> AddHalTimeZoneAsync(HalTimeZone newHalInTimeZone, CancellationToken ct = default);

        Task<IList<HalTimeZone>> GetAllByTimeZoneIdAsync(string timeZoneId, CancellationToken ct = default);

        Task<IList<LeadslyTimeZone>> GetAllSupportedTimeZonesAsync(CancellationToken ct = default);
    }
}
