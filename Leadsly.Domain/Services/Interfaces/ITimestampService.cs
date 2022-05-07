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
        long CreateNowTimestamp();
        Task<DateTime> GetNowLocalizedAsync(string halId, CancellationToken ct = default);
        Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<DateTime> GetStartWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        Task<DateTime> GetEndWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        Task<DateTime> GetLocalizedDateTimeAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default);
        DateTimeOffset GetDateFromTimestamp(long timestamp);
    }
}
