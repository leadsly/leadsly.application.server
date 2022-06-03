﻿using Leadsly.Application.Model.Entities;
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

        public async Task<DateTimeOffset> GetNowLocalizedAsync(string halId, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);
            string zoneId = halUnit.TimeZoneId;

            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            _logger.LogInformation("Executing GetNowLocalizedAsync for zone {zoneId}", zoneId);

            DateTime nowLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);
            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    DateTime.SpecifyKind(nowLocalTime, DateTimeKind.Unspecified
                ),
                tzInfo.GetUtcOffset
                (
                    DateTime.SpecifyKind(nowLocalTime, DateTimeKind.Local)
                ));

            _logger.LogInformation($"Now in local zone {zoneId} is {targetDateTimeOffset}");
            return targetDateTimeOffset;
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

        public async Task<DateTimeOffset> GetStartWorkdayLocalizedForHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving StartWorkDayLocalized for HalId {halId}", halId);

            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            DateTimeOffset startDateTimeOffset = ParseDateTimeOffsetLocalized(halUnit.StartHour, halUnit.TimeZoneId);

            _logger.LogDebug($"Hal with id {halId} start date is {startDateTimeOffset}");

            return startDateTimeOffset;
        }

        public async Task<DateTimeOffset> GetEndWorkDayLocalizedForHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving EndWorkDayLocalized for HalId {halId}", halId);
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            DateTimeOffset endDateTimeOffset = ParseDateTimeOffsetLocalized(halUnit.EndHour, halUnit.TimeZoneId);

            _logger.LogDebug($"Hal with id {halId} end date is {endDateTimeOffset}");

            return endDateTimeOffset;
        }
        
        public async Task<DateTimeOffset> ParseDateTimeOffsetLocalizedAsync(string halId, string timeOfDay, CancellationToken ct = default)
        {
            HalUnit halUnit = await GetHalUnitByHalIdAsync(halId, ct);

            DateTimeOffset targetDateTimeOffset = ParseDateTimeOffsetLocalized(timeOfDay, halUnit.TimeZoneId);

            return targetDateTimeOffset;
        }

        private DateTimeOffset ParseDateTimeOffsetLocalized(string timeOfDay, string timeZoneId)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            TimeSpan ts = DateTime.Parse(timeOfDay).TimeOfDay;
            DateTime nowLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);
            DateTime targetDateTime = nowLocalTime.Date.AddTicks(ts.Ticks);

            DateTimeOffset targetDateTimeOffset =
                new DateTimeOffset
                (
                    targetDateTime,
                    tzInfo.GetUtcOffset
                    (
                        DateTime.SpecifyKind(targetDateTime, DateTimeKind.Local)
                    )
                );

            return targetDateTimeOffset;
        }

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
