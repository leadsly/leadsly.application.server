using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class TimestampService : ITimestampService
    {
        public TimestampService(IHalRepository halRepository, ILogger<TimestampService> logger, IMemoryCache memCache)
        {
            _halRepository = halRepository;
            _logger = logger;
            _memoryCache = memCache;
        }

        private readonly ILogger<TimestampService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IHalRepository _halRepository;

        public long CreateNowTimestamp()
        {
            _logger.LogInformation("Executing CreateNowTimestamp");
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        //public async Task<DateTime> GetNowLocalizedAsync(string halId, CancellationToken ct = default)
        //{
        //    HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);
        //    string zoneId = halUnit.TimeZoneId;

        //    _logger.LogInformation("Executing GetDateTimeNowWithZone for zone {zoneId}", zoneId);
        //    DateTime nowLocal = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, zoneId);
        //    _logger.LogInformation($"Now in local zone {zoneId} is {nowLocal.Date}");

        //    return nowLocal;
        //}

        public async Task<DateTimeOffset> GetNowLocalizedAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);
            string zoneId = halUnit.TimeZoneId;

            _logger.LogInformation("Executing GetDateTimeNowWithZone for zone {zoneId}", zoneId);
            DateTimeOffset nowLocal = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, zoneId);
            _logger.LogInformation($"Now in local zone {zoneId} is {nowLocal}");

            return nowLocal;
        }

        public async Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing GetStartWorkDayTimestamp for HalId {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);
            if(DateTime.TryParse(halUnit.StartHour, out DateTime startDateTime) == false)
            {
                string startHour = halUnit?.StartHour;
                _logger.LogError("Failed to parse hals start hour. Attempted to parse {startHour}", startHour);
            }
            _logger.LogDebug("Successfully parsed start hour date time");
            if(halUnit.TimeZoneId == null)
            {
                _logger.LogError("Hal does NOT have configured time zone! This is required to create time stamp for 'Now' for this hal unit");
            }

            long timestamp = new DateTimeOffset(startDateTime).ToUnixTimeSeconds();

            return timestamp;
        }

        public async Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing GetEndWorkDayTimestamp for HalId {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);            
            if (DateTime.TryParse(halUnit.EndHour, out DateTime endDateTime) == false)
            {
                string endHour = halUnit?.EndHour;
                _logger.LogError("Failed to parse hals end hour. Attempted to parse {endHour}", endHour);
            }
            _logger.LogDebug("Successfully parsed end hour date time");
            if (halUnit.TimeZoneId == null)
            {
                _logger.LogError("Hal does NOT have configured time zone! This is required to create time stamp for 'Now' for this hal unit");
            }

            long timestamp = new DateTimeOffset(endDateTime).ToUnixTimeSeconds();

            return timestamp;
        }

        public async Task<DateTimeOffset> GetDateFromTimestampLocalizedAsync(string halId, long timestamp, CancellationToken ct = default)
        {
            _logger.LogInformation("Converting timestamp {timestamp} to DateTimeOffset", timestamp);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            string tzId = halUnit.TimeZoneId;
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            DateTimeOffset timestampDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(timestampDateTimeOffset, tzInfo);

            return targetTime;
        }

        //public async Task<DateTime> GetStartWorkDayLocalizedAsync(string halId, CancellationToken ct = default)
        //{
        //    _logger.LogInformation("Retrieving StartWorkDayLocalized for HalId {halId}", halId);
        //    HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

        //    if (DateTime.TryParse(halUnit.StartHour, out DateTime startDateTime) == false)
        //    {
        //        string startHour = halUnit?.StartHour;
        //        _logger.LogError("Failed to parse hals start hour. Attempted to parse {startHour}", startHour);
        //    }

        //    _logger.LogDebug($"Successfully created start datetime localized: {startDateTime.Date}");

        //    return startDateTime;
        //}

        public async Task<DateTimeOffset> GetStartWorkDayLocalizedAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving StartWorkDayLocalized for HalId {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            if (DateTimeOffset.TryParse(halUnit.StartHour, out DateTimeOffset startDateTime) == false)
            {
                string startHour = halUnit?.StartHour;
                _logger.LogError("Failed to parse hals start hour. Attempted to parse {startHour}", startHour);
                throw new Exception("Failed to convert hal's start time to DateTimeOffset");
            }

            _logger.LogDebug($"Successfully created start datetime localized: {startDateTime.Date}");
            string tzId = halUnit.TimeZoneId;
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(startDateTime, tzInfo);

            return targetTime;
        }

        //public async Task<DateTime> GetEndWorkDayLocalizedAsync(string halId, CancellationToken ct = default)
        //{
        //    _logger.LogInformation("Retrieving EndWorkDayLocalized for HalId {halId}", halId);
        //    HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

        //    if (DateTime.TryParse(halUnit.EndHour, out DateTime endDateTime) == false)
        //    {
        //        string endHour = halUnit?.EndHour;
        //        _logger.LogError("Failed to parse hals end hour. Attempted to parse {endHour}", endHour);
        //    }

        //    _logger.LogDebug($"Successfully created end datetime localized: {endDateTime.Date}");

        //    return endDateTime;
        //}

        public async Task<DateTimeOffset> GetEndWorkDayLocalizedAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving EndWorkDayLocalized for HalId {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            if (DateTime.TryParse(halUnit.EndHour, out DateTime endDateTime) == false)
            {
                string endHour = halUnit?.EndHour;
                _logger.LogError("Failed to parse hals end hour. Attempted to parse {endHour}", endHour);
            }

            _logger.LogDebug($"Successfully created end datetime localized: {endDateTime}");

            string tzId = halUnit.TimeZoneId;
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(endDateTime, tzInfo);

            return targetTime;
        }

        //public async Task<DateTime> GetLocalizedDateTimeAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default)
        //{
        //    _logger.LogInformation($"Creating localized date time from DateTimeOffset: {dateTimeOffset.DateTime}");

        //    _logger.LogDebug("Retrieving hal with id {halId}", halId);
        //    HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

        //    string timeZoneId = halUnit.TimeZoneId;
        //    _logger.LogDebug("HalUnit's time zone id is {timeZoneId}", timeZoneId);
        //    DateTime localDateTime = new DateTimeWithZone(dateTimeOffset.DateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)).LocalTime;

        //    _logger.LogInformation($"Successfully created localized date time {localDateTime.Date}");

        //    return localDateTime;
        //}

        public async Task<DateTimeOffset> GetLocalizedDateTimeOffsetAsync(string halId, DateTimeOffset dateTimeOffset, CancellationToken ct = default)
        {
            _logger.LogInformation($"Creating localized DateTimeOffset from DateTimeOffset: {dateTimeOffset}");

            _logger.LogDebug("Retrieving hal with id {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            string timeZoneId = halUnit.TimeZoneId;
            _logger.LogDebug("HalUnit's time zone id is {timeZoneId}", timeZoneId);
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);                        

            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);

            return targetTime;
        }

        private async Task<HalUnit> GetHalUnitByHalIdAsync(string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(halId, out HalUnit halUnit) == false)
            {
                halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
                _memoryCache.Set(halId, halUnit, DateTimeOffset.Now.AddMinutes(3));
            }
            return halUnit;
        }

    }
}
