using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ITimeZoneRepository
    {
        Task<IList<LeadslyTimeZone>> GetAllSupportedTimeZonesAsync(CancellationToken ct = default);
    }
}
