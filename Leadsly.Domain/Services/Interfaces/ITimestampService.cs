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
        //Task<DateTime> GetNowLocalizedAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetNowLocalizedAsync(string halId, CancellationToken ct = default);
        Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        //Task<DateTime> GetStartWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetStartWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        //Task<DateTime> GetEndWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetEndWorkDayLocalizedAsync(string halId, CancellationToken ct = default);
        //Task<DateTime> GetLocalizedDateTimeAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default);
        Task<DateTimeOffset> GetLocalizedDateTimeOffsetAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default);
        Task<DateTimeOffset> GetDateFromTimestampLocalizedAsync(string halId, long timestamp, CancellationToken ct = default);
    }
}
