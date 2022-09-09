using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class RestartHalsByTimezoneJobService : IRestartHalsByTimezoneJobService
    {
        public RestartHalsByTimezoneJobService(IHalRepository halRepository, ITimestampService timestampService, ILogger<RestartHalJobService> logger, IHangfireService hangfireService)
        {
            _halRepository = halRepository;
            _timestampService = timestampService;
            _logger = logger;
            _hangfireSerivce = hangfireService;
        }

        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<RestartHalJobService> _logger;
        private readonly IHangfireService _hangfireSerivce;

        public async Task PublishRestartJobsPerTimezoneAsync(string timeZoneId)
        {
            IList<HalUnit> halsInTimezone = await _halRepository.GetAllByTimeZoneIdAsync(timeZoneId);
            if (halsInTimezone.Count > 0)
            {
                int hals = halsInTimezone.Count;
                _logger.LogDebug("Time zone {timeZoneId}, has {hals} hal units.", timeZoneId, hals);

                foreach (HalUnit halUnit in halsInTimezone)
                {
                    // Grab this Hal's start time and schedule the job shortly after the start time                    
                    string halId = halUnit.HalId;
                    _logger.LogInformation("Hal unit with id {halId} was found", halId);
                    DateTimeOffset startDate = _timestampService.ParseDateTimeOffsetLocalized(halUnit.TimeZoneId, halUnit.StartHour);
                    _logger.LogInformation($"Hal unit with id {halId}, has a start date of {startDate}");

                    // restart hal one hour before all phases are expected to start executing
                    DateTimeOffset restartStartDate = startDate.AddMinutes(-60);
                    _logger.LogInformation($"Hal unit with id {halId}, has a restart date of {restartStartDate}");
                    _logger.LogDebug("Scheduling RestartHalAsync for halId {halId}", halId);
                    _hangfireSerivce.Schedule<IRestartHalJobService>((x) => x.RestartHalAsync(halId), restartStartDate);
                }
            }
            else
            {
                _logger.LogInformation("No hal units found for time zone {timeZoneId}", timeZoneId);
            }
        }
    }
}
