using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ITimestampService
    {
        Task<long> CreateNowTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> CreateStartWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> CreateEndWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetStartWorkDayAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetEndWorkDayAsync(string halId, CancellationToken ct = default);
    }
}
