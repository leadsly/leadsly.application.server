using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ITimestampService
    {
        long CreateNowTimestamp();        
        Task<DateTimeOffset> GetNowLocalizedAsync(string halId, CancellationToken ct = default);      
        Task<DateTimeOffset> GetStartWorkdayLocalizedForHalIdAsync(string halId, CancellationToken ct = default);                
        Task<DateTimeOffset> GetEndWorkDayLocalizedForHalIdAsync(string halId, CancellationToken ct = default);
        DateTimeOffset ParseDateTimeOffsetLocalized(string timeZoneId, string timeOfDay);
        Task<DateTimeOffset> GetLocalizedDateTimeOffsetAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default);
        Task<DateTimeOffset> GetDateFromTimestampLocalizedAsync(string halId, long timestamp, CancellationToken ct = default);        
    }
}
