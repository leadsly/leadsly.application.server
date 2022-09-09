using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Domain.JobServices.Interfaces;
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
    public class NewTimeZonesJobsService : INewTimeZonesJobsService
    {
        public NewTimeZonesJobsService(
            ILogger<NewTimeZonesJobsService> logger,
            ITimeZoneRepository timezoneRepository,
            IHangfireService hangfireService,
            IWebHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _hangfireService = hangfireService;
            _timezoneRepository = timezoneRepository;
        }

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<NewTimeZonesJobsService> _logger;
        private readonly IHangfireService _hangfireService;
        private readonly ITimeZoneRepository _timezoneRepository;

        public async Task AddRecurringJobsForNewTimeZonesAsync()
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
                        _hangfireService.Enqueue<ICampaignJobsService>(x => x.ExecuteDailyJobsAsync(timeZoneId));
                    }
                    else
                    {
                        if (recurringJob == null || recurringJob?.Count == 0)
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs for active campaigns");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<ICampaignJobsService>(jobName, (x) => x.ExecuteDailyJobsAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }
                        else
                        {
                            _logger.LogInformation("Recurring job was found. No need to create a new recurring job");
                        }
                    }
                }
            }
        }

        public async Task AddRecurringRestartJobsForNewTimeZonesAsync()
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
                            _hangfireService.Enqueue<IRestartHalsByTimezoneJobService>(x => x.PublishRestartJobsPerTimezoneAsync(timeZoneId));
                        }
                        else
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs to restart all registered hal units");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<IRestartHalsByTimezoneJobService>(jobName, (x) => x.PublishRestartJobsPerTimezoneAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
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
