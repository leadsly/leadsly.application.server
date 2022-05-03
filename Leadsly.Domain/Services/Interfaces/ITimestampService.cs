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
        Task<DateTimeOffset> CreateNowDatetimeOffsetAsync(string halId, CancellationToken ct = default);
        Task<long> CreateTimestampInZoneAsync(string halId, long timestamp, CancellationToken ct = default);
        Task<DateTimeOffset> CreateDatetimeOffsetAsync(string halId, DateTime date, CancellationToken ct = default);
        Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetStartWorkDayAsync(string halId, CancellationToken ct = default);
        Task<DateTimeOffset> GetEndWorkDayAsync(string halId, CancellationToken ct = default);
        DateTimeOffset GetDateFromTimestamp(long timestamp);
        bool TryParseString(string dateTime, out DateTimeOffset dateTimeOffset);
    }
}
