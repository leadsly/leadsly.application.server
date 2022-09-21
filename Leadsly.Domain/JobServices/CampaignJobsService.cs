﻿using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class CampaignJobsService : ICampaignJobsService
    {
        public CampaignJobsService(
            ILogger<CampaignJobsService> logger,
            ITimestampService timestampService,
            IHalRepository halRepository,
            IWebHostEnvironment env,
            IHangfireService hangfireService
            )
        {
            _env = env;
            _hangfireService = hangfireService;
            _logger = logger;
            _timestampService = timestampService;
            _halRepository = halRepository;
        }

        private readonly IWebHostEnvironment _env;
        private readonly IHangfireService _hangfireService;
        private readonly ILogger<CampaignJobsService> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IHalRepository _halRepository;

        public async Task ExecuteDailyJobsAsync(string timezoneId)
        {
            // get all hal ids for this time zone
            _logger.LogInformation("Executing daily cron job for time zone {timeZoneId}", timezoneId);

            IList<HalUnit> halsInTimezone = await _halRepository.GetAllByTimeZoneIdAsync(timezoneId);
            if (halsInTimezone.Count > 0)
            {
                int hals = halsInTimezone.Count;
                _logger.LogDebug("Time zone {timeZoneId}, has {hals} hal units.", timezoneId, hals);

                foreach (HalUnit halUnit in halsInTimezone)
                {
                    // Grab this Hal's start time and schedule the job shortly after the start time                    
                    string halId = halUnit.HalId;
                    _logger.LogInformation("Hal unit with id {halId} was found", halId);
                    DateTimeOffset startDate = _timestampService.ParseDateTimeOffsetLocalized(halUnit.TimeZoneId, halUnit.StartHour);
                    _logger.LogInformation($"Hal unit with id {halId}, has a start date of {startDate}");

                    DateTimeOffset checkOffHoursNewConnectionsStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishCheckOffHoursNewConnectionsAsync for halId {halId}. Start time for this phase is {checkOffHoursNewConnectionsStartDate}", halId, checkOffHoursNewConnectionsStartDate);
                    ExecuteCheckOffHoursNewConnections(halId, checkOffHoursNewConnectionsStartDate);

                    DateTimeOffset prospectPhaseStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishProspectingPhaseAsync for halId {halId}. Start time for this phase is {prospectPhaseStartDate}", halId, prospectPhaseStartDate);
                    ExecuteProspectingPhase(halId, prospectPhaseStartDate);

                    DateTimeOffset monitorForNewConnectionsStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishMonitorForNewConnectionsAsync for halId {halId}. Start time for this phase is {monitorForNewConnectionsStartDate}", halId, monitorForNewConnectionsStartDate);
                    ExecuteMonitorForNewConnections(halId, monitorForNewConnectionsStartDate);

                    // this doesn't actually trigger anything when it executes, it just schedules the next jobs to execute.
                    _logger.LogDebug("Enqueuing PublishNetworkingPhaseAsync for halId {halId}. This will be enqueued right now", halId);
                    _hangfireService.Enqueue<INetworkingJobsService>((x) => x.PublishNetworkingMQMessagesAsync(halId));

                    _logger.LogDebug("Enqueing PreventFollowUpMessageService for halId {halId}", halId);
                    _hangfireService.Enqueue<IPreventFollowUpMessageService>((x) => x.MarkProspectsAsCompleteAsync(halId));
                }
            }
            else
            {
                _logger.LogInformation("No hal units found for time zone {timeZoneId}", timezoneId);
            }
        }

        private void ExecuteCheckOffHoursNewConnections(string halId, DateTimeOffset checkOffHoursNewConnectionsStartDate)
        {
            if (_env.IsDevelopment())
            {
                _hangfireService.Enqueue<ICheckOffHoursNewConnectionsJobService>((x) => x.PublishCheckOffHoursNewConnectionsMQMessageAsync(halId));
            }
            else
            {
                _hangfireService.Schedule<ICheckOffHoursNewConnectionsJobService>((x) => x.PublishCheckOffHoursNewConnectionsMQMessageAsync(halId), checkOffHoursNewConnectionsStartDate);
            }
        }

        private void ExecuteProspectingPhase(string halId, DateTimeOffset prospectPhaseStartDate)
        {
            if (_env.IsDevelopment())
            {
                _hangfireService.Enqueue<IProspectingJobService>((x) => x.PublishProspectingMQMessagesAsync(halId));
            }
            else
            {
                _hangfireService.Schedule<IProspectingJobService>((x) => x.PublishProspectingMQMessagesAsync(halId), prospectPhaseStartDate);
            }
        }

        private void ExecuteMonitorForNewConnections(string halId, DateTimeOffset monitorForNewConnectionsStartDate)
        {
            if (_env.IsDevelopment())
            {
                _hangfireService.Enqueue<IMonitorForNewConnectionsJobService>((x) => x.PublishMonitorForNewConnectionsMQMessagesAsync(halId));
            }
            else
            {
                _hangfireService.Schedule<IMonitorForNewConnectionsJobService>((x) => x.PublishMonitorForNewConnectionsMQMessagesAsync(halId), monitorForNewConnectionsStartDate);
            }
        }
    }
}