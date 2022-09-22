using Hangfire;
using Hangfire.Storage;
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

        public async Task AddRecurringJobsForNewTimeZonesAsync_AllInOneVirtualAssistant()
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timezoneRepository.GetAllSupportedTimeZonesAsync();
            _logger.LogDebug("{0} found {1} supported time zones", nameof(AddRecurringJobsForNewTimeZonesAsync_AllInOneVirtualAssistant), supportedTimeZones.Count);

            foreach (LeadslyTimeZone leadslyTimeZone in supportedTimeZones)
            {
                string timeZoneId = leadslyTimeZone.TimeZoneId;
                _logger.LogTrace("Currently executing job is for for {timeZoneId}", timeZoneId);

                if (_env.IsDevelopment())
                {
                    _hangfireService.Enqueue<ICampaignJobsService>(x => x.ExecuteDailyJobsAsync_AllInOneVirtualAssistant(timeZoneId));
                }
                else
                {
                    string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);
                    string jobName = $"ActiveCampaigns_AllInOneVirtualAssistant_{timeZoneIdNormalized}";
                    _logger.LogTrace("Recurring daily Hangfire job name is {jobName}", jobName);

                    string hangfireRecurringJobName = $"recurring-job:{jobName}";
                    _logger.LogDebug($"Retrieving Hangfire recurring job using the following hash name {hangfireRecurringJobName}");
                    using (var connection = JobStorage.Current.GetConnection())
                    {
                        Dictionary<string, string> recurringJob = connection.GetAllEntriesFromHash(hangfireRecurringJobName);

                        _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {jobName}", jobName);

                        if (recurringJob == null || recurringJob?.Count == 0)
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs for active campaigns");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<ICampaignJobsService>(jobName, (x) => x.ExecuteDailyJobsAsync_AllInOneVirtualAssistant(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }
                        else
                        {
                            _logger.LogInformation("Recurring job was found. No need to create a new recurring job");
                        }
                    }
                }
            }
        }

        public async Task AddRecurringJobsForNewTimeZonesAsync()
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timezoneRepository.GetAllSupportedTimeZonesAsync();
            _logger.LogDebug("{0} found {1} supported time zones", nameof(AddRecurringJobsForNewTimeZonesAsync), supportedTimeZones.Count);

            foreach (LeadslyTimeZone leadslyTimeZone in supportedTimeZones)
            {
                string timeZoneId = leadslyTimeZone.TimeZoneId;
                _logger.LogTrace("Currently executing job is for for {timeZoneId}", timeZoneId);

                if (_env.IsDevelopment())
                {
                    _hangfireService.Enqueue<ICampaignJobsService>(x => x.ExecuteDailyJobsAsync(timeZoneId));

                    _hangfireService.Enqueue<ICampaignJobsService>((x) => x.RunMarkProspectsAsCompleteAsyncDailyJobAsync(timeZoneId));
                }
                else
                {
                    using (var connection = JobStorage.Current.GetConnection())
                    {
                        AddOrUpdateDailyJob(timeZoneId, connection);

                        AddOrUpdateMarkProspectsAsCompleteJobs(timeZoneId, connection);
                    }
                }
            }
        }

        private void AddOrUpdateMarkProspectsAsCompleteJobs(string timeZoneId, IStorageConnection connection)
        {
            string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);

            string markProspectsAsCompleteJobName = $"MarkProspectsAsComplete_{timeZoneIdNormalized}";
            _logger.LogTrace("Recurring daily Hangfire job name is {markProspectsAsCompleteJobName}", markProspectsAsCompleteJobName);

            string markProspectsAsCompleteHangfireRecurringJobName = $"recurring-job:{markProspectsAsCompleteJobName}";
            _logger.LogDebug($"Retrieving Hangfire recurring job using the following hash name {markProspectsAsCompleteHangfireRecurringJobName}");
            Dictionary<string, string> markProspectsAsCompleteRecurringJob = connection.GetAllEntriesFromHash(markProspectsAsCompleteHangfireRecurringJobName);

            _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {markProspectsAsCompleteJobName}", markProspectsAsCompleteJobName);

            if (markProspectsAsCompleteRecurringJob == null || markProspectsAsCompleteRecurringJob?.Count == 0)
            {
                _logger.LogInformation("New daily Cron job is being added. This will go through all supported time zones and triggers jobs to mark campaign prospects as complete if applicable");
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                _hangfireService.AddOrUpdate<ICampaignJobsService>(markProspectsAsCompleteJobName, (x) => x.RunMarkProspectsAsCompleteAsyncDailyJobAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
            }
            else
            {
                _logger.LogInformation("Recurring job {0} was found. No need to create a new recurring job", markProspectsAsCompleteJobName);
            }
        }

        private void AddOrUpdateDailyJob(string timeZoneId, IStorageConnection connection)
        {
            string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);

            string activeCampaignsJobName = $"ActiveCampaigns_{timeZoneIdNormalized}";
            _logger.LogTrace("Recurring daily Hangfire job name is {activeCampaignsJobName}", activeCampaignsJobName);

            string activeCampaignsHangfireRecurringJobName = $"recurring-job:{activeCampaignsJobName}";
            _logger.LogDebug($"Retrieving Hangfire recurring job using the following hash name {activeCampaignsHangfireRecurringJobName}");
            Dictionary<string, string> activeCampaignsRecurringJob = connection.GetAllEntriesFromHash(activeCampaignsHangfireRecurringJobName);

            _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {activeCampaignsJobName}", activeCampaignsJobName);

            if (activeCampaignsRecurringJob == null || activeCampaignsRecurringJob?.Count == 0)
            {
                _logger.LogInformation("New daily Cron job is being added. This will go through all supported time zones and triggers jobs for active campaigns when it is due");
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                _hangfireService.AddOrUpdate<ICampaignJobsService>(activeCampaignsJobName, (x) => x.ExecuteDailyJobsAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
            }
            else
            {
                _logger.LogInformation("Recurring job {0} was found. No need to create a new recurring job", activeCampaignsJobName);
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
