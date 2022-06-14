﻿using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            ILeadslyRecurringJobsManagerService leadslyRecurringJobsManagersService,
            IWebHostEnvironment env)
        {
            _env = env;
            _serviceProbider = serviceProbider;
            _leadslyRecurringJobsManagerService = leadslyRecurringJobsManagersService;
            _hangfireService = hangfireService;
            _timezoneRepository = timezoneRepository;
        }

        private readonly IWebHostEnvironment _env;
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
            IList<HalTimeZone> halTimeZones = await _timezoneRepository.GetAllByTimeZoneIdAsync(timeZoneId);
            if(halTimeZones.Count > 0)
            {
                List<string> halIds = halTimeZones.Select(h => h.HalId).ToList();

                foreach (string halId in halIds)
                {
                    await _leadslyRecurringJobsManagerService.PublishMessagesAsync(halId);
                }
            }            
        }

        public async Task ScheduleJobsForNewTimeZonesAsync()
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timezoneRepository.GetAllSupportedTimeZonesAsync();

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (LeadslyTimeZone leadslyTimeZone in supportedTimeZones)
                {
                    string timeZoneId = leadslyTimeZone.TimeZoneId;
                    string timeZoneIdNormalized = timeZoneId.Trim().Replace(" ", string.Empty);
                    string jobName = $"ActiveCampaigns_{timeZoneIdNormalized}";
                    Dictionary<string, string> recurringJob = connection.GetAllEntriesFromHash($"recurring-job:{jobName}");

                    if(recurringJob == null || recurringJob?.Count == 0)
                    {
                        if (_env.IsDevelopment())
                        {
                            _hangfireService.Enqueue<IRecurringJobsHandler>(x => x.PublishJobsAsync(timeZoneId));
                        }
                        else
                        {
                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                            _hangfireService.AddOrUpdate<IRecurringJobsHandler>(jobName, (x) => x.PublishJobsAsync(timeZoneId), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
                        }                        
                    }
                }
            }                
        }
    }
}
