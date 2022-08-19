using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RecurringJobsHandler : IRecurringJobsHandler
    {
        public RecurringJobsHandler(
            ITimeZoneRepository timezoneRepository,
            IHangfireService hangfireService,
            IHalRepository halRepository,
            ITimestampService timestampService,
            ILogger<RecurringJobsHandler> logger,
            IWebHostEnvironment env)
        {
            _env = env;
            _halRepository = halRepository;
            _logger = logger;
            _timestampService = timestampService;
            _hangfireService = hangfireService;
            _timezoneRepository = timezoneRepository;
        }

        private readonly IWebHostEnvironment _env;
        private readonly ITimestampService _timestampService;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<RecurringJobsHandler> _logger;
        private readonly IHangfireService _hangfireService;
        private readonly ITimeZoneRepository _timezoneRepository;

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
                    _hangfireService.Schedule<ILeadslyRecurringJobsManagerService>((x) => x.RestartHalAsync(halId), restartStartDate);
                }
            }
            else
            {
                _logger.LogInformation("No hal units found for time zone {timeZoneId}", timeZoneId);
            }
        }

        /// <summary>
        /// This method is executed once a day at 4:40AM of the currently executing timezone. 
        /// It grabs all of HalUnits registered in this timezone parses out each Hal unit start hour (7:00AM for example)
        /// and schedules a job to go out at that time.
        /// </summary>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        public async Task PublishJobsAsync(string timeZoneId)
        {
            // get all hal ids for this time zone
            _logger.LogInformation("Executing daily cron job for time zone {timeZoneId}", timeZoneId);

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

                    DateTimeOffset checkOffHoursNewConnectionsStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishCheckOffHoursNewConnectionsAsync for halId {halId}. Start time for this phase is {checkOffHoursNewConnectionsStartDate}", halId, checkOffHoursNewConnectionsStartDate);
                    _hangfireService.Schedule<ILeadslyRecurringJobsManagerService>((x) => x.PublishCheckOffHoursNewConnectionsAsync(halId), checkOffHoursNewConnectionsStartDate);

                    DateTimeOffset prospectPhaseStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishProspectingPhaseAsync for halId {halId}. Start time for this phase is {prospectPhaseStartDate}", halId, prospectPhaseStartDate);
                    _hangfireService.Schedule<ILeadslyRecurringJobsManagerService>((x) => x.PublishProspectingPhaseAsync(halId), prospectPhaseStartDate);

                    DateTimeOffset monitorForNewConnectionsStartDate = startDate;
                    _logger.LogDebug("Scheduling PublishMonitorForNewConnectionsAsync for halId {halId}. Start time for this phase is {monitorForNewConnectionsStartDate}", halId, monitorForNewConnectionsStartDate);
                    _hangfireService.Schedule<ILeadslyRecurringJobsManagerService>((x) => x.PublishMonitorForNewConnectionsAsync(halId), monitorForNewConnectionsStartDate);

                    // this doesn't actually trigger anything when it executes, it just schedules the next jobs to execute.
                    DateTimeOffset enqueueNetworkingStartDate = await _timestampService.GetNowLocalizedAsync(halId);
                    _logger.LogDebug("Enqueuing PublishNetworkingPhaseAsync for halId {halId}. This will be enqueued right now. Currently it is: {enqueueNetworkingStartDate}", halId, enqueueNetworkingStartDate);
                    _hangfireService.Enqueue<ILeadslyRecurringJobsManagerService>((x) => x.PublishNetworkingPhaseAsync(halId));
                }
            }
            else
            {
                _logger.LogInformation("No hal units found for time zone {timeZoneId}", timeZoneId);
            }
        }

        /// <summary>
        /// This method is executed each time the application starts. Upon start it will get all of the entries from the 'SupportedTimeZones' table.
        /// It will then check to see if there are any existing jobs in the database that match the following format 'ActiveCampaigns_EasternStandardTime'.
        /// Any new timezones will be then added as a recurring job based on the CRON schedule. As of right now it is 4:40AM of whatever timezone is being added.
        /// If new timezone is added to the database it will be picked up the next day this method is scheduled to execute.
        /// </summary>
        /// <returns></returns>
        public async Task ScheduleJobsForNewTimeZonesAsync()
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timezoneRepository.GetAllSupportedTimeZonesAsync();
            _logger.LogDebug($"'ScheduleJobsForNewTimeZonesAsync' found {supportedTimeZones.Count} supported time zones");

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (LeadslyTimeZone leadslyTimeZone in supportedTimeZones)
                {
                    string timeZoneId = leadslyTimeZone.TimeZoneId;
                    _logger.LogTrace("Currently executing job is for for {timeZoneId}", timeZoneId);
                    string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);
                    string jobName = $"ActiveCampaigns_{timeZoneIdNormalized}";
                    _logger.LogTrace("Recurring daily Hangfire job name is {jobName}", jobName);

                    string hangfireRecurringJobName = $"recurring-job:{jobName}";
                    _logger.LogDebug($"Retrieving Hangfire recurring job using the following hash name {hangfireRecurringJobName}");
                    Dictionary<string, string> recurringJob = connection.GetAllEntriesFromHash(hangfireRecurringJobName);

                    _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {jobName}", jobName);
                    if (_env.IsDevelopment())
                    {
                        _hangfireService.Enqueue<IRecurringJobsHandler>(x => x.PublishJobsAsync(timeZoneId));
                    }
                    else
                    {
                        if (recurringJob == null || recurringJob?.Count == 0)
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs for active campaigns");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<IRecurringJobsHandler>(jobName, (x) => x.PublishJobsAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }
                        else
                        {
                            _logger.LogInformation("Recurring job was found. No need to create a new recurring job");
                        }
                    }
                }
            }
        }

        public async Task ScheduleRestartJobsForNewTimeZonesAsync()
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timezoneRepository.GetAllSupportedTimeZonesAsync();
            _logger.LogDebug($"'ScheduleRestartJobsForNewTimeZonesAsync' found {supportedTimeZones.Count} supported time zones");

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (LeadslyTimeZone leadslyTimeZone in supportedTimeZones)
                {
                    string timeZoneId = leadslyTimeZone.TimeZoneId;
                    _logger.LogTrace("Currently executing job is for for {timeZoneId}", timeZoneId);
                    string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);
                    string jobName = $"RestartHal_{timeZoneIdNormalized}";
                    _logger.LogTrace("Recurring daily Hangfire job name is {jobName}", jobName);

                    string hangfireRecurringJobName = $"recurring-job:{jobName}";
                    _logger.LogDebug($"Retrieving Hangfire recurring job using the following hash name {hangfireRecurringJobName}");
                    Dictionary<string, string> recurringJob = connection.GetAllEntriesFromHash(hangfireRecurringJobName);

                    if (recurringJob == null || recurringJob?.Count == 0)
                    {
                        _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {jobName}", jobName);
                        if (_env.IsDevelopment())
                        {
                            _hangfireService.Enqueue<IRecurringJobsHandler>(x => x.PublishRestartJobsPerTimezoneAsync(timeZoneId));
                        }
                        else
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs to restart all registered hal units");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<IRecurringJobsHandler>(jobName, (x) => x.PublishRestartJobsPerTimezoneAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Recurring job was found. No need to create a new recurring job");
                    }
                }
            }
        }
    }
}
