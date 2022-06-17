using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RecurringJobsHandler : IRecurringJobsHandler
    {
        public RecurringJobsHandler(IServiceProvider serviceProbider,
            ITimeZoneRepository timezoneRepository, 
            IHangfireService hangfireService,
            IHalRepository halRepository,
            ITimestampService timestampService,
            ILeadslyRecurringJobsManagerService leadslyRecurringJobsManagersService,
            ILogger<RecurringJobsHandler> logger,
            IWebHostEnvironment env)
        {
            _env = env;
            _halRepository = halRepository;
            _logger = logger;
            _timestampService = timestampService;
            _serviceProbider = serviceProbider;
            _leadslyRecurringJobsManagerService = leadslyRecurringJobsManagersService;
            _hangfireService = hangfireService;
            _timezoneRepository = timezoneRepository;
        }

        private readonly IWebHostEnvironment _env;
        private readonly ITimestampService _timestampService;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<RecurringJobsHandler> _logger;
        private readonly IServiceProvider _serviceProbider;
        private readonly IHangfireService _hangfireService;
        private readonly ITimeZoneRepository _timezoneRepository;
        private readonly ILeadslyRecurringJobsManagerService _leadslyRecurringJobsManagerService;

        public async Task CreateAndPublishJobsAsync()
        {
            using (var scope = _serviceProbider.CreateScope())
            {
                //////////////////////////////////////////////////////////////////////////////////////
                ///// ScanForNewConnectionsOffHours
                //////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler =
                    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand>>();

                CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand();
                await offHoursHandler.HandleAsync(offHoursCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// MonitorForNewConnectionsAll
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand> monitorHandler =
                    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand>>();

                MonitorForNewConnectionsAllCommand monitorForNewConnectionsAllCommand = new MonitorForNewConnectionsAllCommand();
                await monitorHandler.HandleAsync(monitorForNewConnectionsAllCommand);

                ////////////////////////////////////////////////////////////////////////////////////////
                /////// UncontactedFollowUpMessageCommand
                ////////////////////////////////////////////////////////////////////////////////////////
                ////HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand> uncontactedHandler =
                ////   scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand>>();

                ////UncontactedFollowUpMessageCommand uncontactedCommand = new UncontactedFollowUpMessageCommand();
                ////await uncontactedHandler.HandleAsync(uncontactedCommand);

                IPhaseManager phaseManager = scope.ServiceProvider.GetRequiredService<IPhaseManager>();

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                /// Prospecting [ DeepScanProspectsForReplies OR (FollowUpMessagePhase AND ScanProspectsForReplies) ]
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                await phaseManager.ProspectingPhaseAsync();

                ////////////////////////////////////////////////////////////////////////////////////////////////
                /// NetworkingConnectionsPhase[ProspectListPhase OR SendConnectionsPhase]
                ////////////////////////////////////////////////////////////////////////////////////////////////
                await phaseManager.NetworkingConnectionsPhaseAsync();

                ////////////////////////////////////////////////////////////////////////////////////////////////
                /// NetworkingPhase[ProspectListPhase AND SendConnectionsPhase Merged]
                ////////////////////////////////////////////////////////////////////////////////////////////////
                await phaseManager.NetworkingPhaseAsync();
            }
        }

        public async Task PublishJobsAsync(string timeZoneId)
        {
            // get all hal ids for this time zone
            _logger.LogInformation("Executing daily cron job for time zone {timeZoneId}", timeZoneId);
            IList<HalTimeZone> halTimeZones = await _timezoneRepository.GetAllByTimeZoneIdAsync(timeZoneId);
            if(halTimeZones.Count > 0)
            {
                int hals = halTimeZones.Count;
                _logger.LogDebug("Time zone {timeZoneId}, has {hals} hal units.", timeZoneId, hals);

                List<string> halIds = halTimeZones.Select(h => h.HalId).ToList();
                foreach (string halId in halIds)
                {
                    // Grab this Hal's start time and schedule the job shortly after the start time
                    HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);
                    if(halUnit != null)
                    {
                        _logger.LogInformation("Hal unit with id {halId} was found", halId);
                        DateTimeOffset startDate = _timestampService.ParseDateTimeOffsetLocalized(halUnit.TimeZoneId, halUnit.StartHour);                        
                        _logger.LogInformation($"Hal unit with id {halId}, has a start date of {startDate}");
                        _hangfireService.Schedule<ILeadslyRecurringJobsManagerService>((x) => x.PublishHalPhasesAsync(halId), startDate);
                    }
                }
            }            
        }

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

                    if(recurringJob == null || recurringJob?.Count == 0)
                    {
                        _logger.LogDebug("Recurring job was not found. Creating a new recurring job called {jobName}", jobName);
                        if (_env.IsDevelopment())
                        {
                            _hangfireService.Enqueue<IRecurringJobsHandler>(x => x.PublishJobsAsync(timeZoneId));
                        }
                        else
                        {
                            _logger.LogInformation("Daily Cron job is about to execute. This will go through all supported time zones and triggers jobs for active campaigns");
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<IRecurringJobsHandler>(jobName, (x) => x.PublishJobsAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }                        
                    }
                }
            }                
        }
    }
}
