using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ITimestampService
    {
        long CreateNowTimestamp();        
        Task<DateTimeOffset> GetNowLocalizedAsync(string halId, CancellationToken ct = default);
        Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default);
        Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default);        
        Task<DateTimeOffset> GetStartOfWorkdayForHalIdAsync(string halId, CancellationToken ct = default);                
        Task<DateTimeOffset> GetEndWorkDayForHalIdAsync(string halId, CancellationToken ct = default);        
        Task<DateTimeOffset> GetLocalizedDateTimeOffsetAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default);
        Task<DateTimeOffset> GetDateFromTimestampLocalizedAsync(string halId, long timestamp, CancellationToken ct = default);
    }
}
