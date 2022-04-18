using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<long> CreateNowTimestampAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId);

            DateTime localDateTime = new DateTimeWithZone(DateTime.Now, timeZoneInfo).LocalTime;

            long timeStamp = new DateTimeOffset(localDateTime).ToUnixTimeSeconds();

            return timeStamp;
        }

        public async Task<DateTimeOffset> CreateNowDatetimeOffsetAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId);

            DateTime localDateTime = new DateTimeWithZone(DateTime.Now, timeZoneInfo).LocalTime;

            DateTimeOffset localDateTimeOffset = new DateTimeOffset(localDateTime);

            return localDateTimeOffset;
        }

        public async Task<DateTimeOffset> CreateDatetimeOffsetAsync(string halId, DateTime date, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId);

            DateTime localDateTime = new DateTimeWithZone(date, timeZoneInfo).LocalTime;

            DateTimeOffset localDateTimeOffset = new DateTimeOffset(localDateTime);

            return localDateTimeOffset;
        }

        public async Task<long> GetStartWorkDayTimestampAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);
            if(DateTime.TryParse(halUnit.StartHour, out DateTime startDateTime) == false)
            {
                string startHour = halUnit?.StartHour;
                _logger.LogError("Failed to parse hals start hour. Attempted to parse {startHour}", startHour);                ;
            }

            if(halUnit.TimeZoneId == null)
            {
                _logger.LogError("Hal does NOT have configured time zone! This is required to create time stamp for 'Now' for this hal unit");
            }

            DateTime localDateTime = new DateTimeWithZone(startDateTime, TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId)).LocalTime;

            long timestamp = new DateTimeOffset(localDateTime).ToUnixTimeSeconds();

            return timestamp;
        }

        public async Task<long> GetEndWorkDayTimestampAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);            

            if (DateTime.TryParse(halUnit.EndHour, out DateTime endDateTime) == false)
            {
                string endHour = halUnit?.EndHour;
                _logger.LogError("Failed to parse hals end hour. Attempted to parse {endHour}", endHour);
            }

            if (halUnit.TimeZoneId == null)
            {
                _logger.LogError("Hal does NOT have configured time zone! This is required to create time stamp for 'Now' for this hal unit");
            }

            DateTime localDateTime = new DateTimeWithZone(endDateTime, TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId)).LocalTime;

            long timestamp = new DateTimeOffset(localDateTime).ToUnixTimeSeconds();

            return timestamp;
        }

        public async Task<DateTimeOffset> GetStartWorkDayAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            if (DateTime.TryParse(halUnit.StartHour, out DateTime startDateTime) == false)
            {
                string startHour = halUnit?.StartHour;
                _logger.LogError("Failed to parse hals start hour. Attempted to parse {startHour}", startHour);
            }

            DateTime localDateTime = new DateTimeWithZone(startDateTime, TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId)).LocalTime;

            DateTimeOffset startDateTimeOffset = new DateTimeOffset(localDateTime);

            return startDateTimeOffset;
        }

        public async Task<DateTimeOffset> GetEndWorkDayAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            if (DateTime.TryParse(halUnit.EndHour, out DateTime endDateTime) == false)
            {
                string endHour = halUnit?.EndHour;
                _logger.LogError("Failed to parse hals end hour. Attempted to parse {endHour}", endHour);
            }

            DateTime localDateTime = new DateTimeWithZone(endDateTime, TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId)).LocalTime;

            DateTimeOffset endDateTimeOffset = new DateTimeOffset(localDateTime);

            return endDateTimeOffset;
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

        public bool TryParseString(string dateTime, out DateTimeOffset dateTimeOffset)
        {
            if(DateTimeOffset.TryParse(dateTime, out dateTimeOffset) == true)
            {
                return true;
            }
            return false;
        }
    }
}
