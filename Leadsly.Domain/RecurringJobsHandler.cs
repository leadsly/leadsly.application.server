using Hangfire;
using Hangfire.Storage;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.UncontactedFollowUpMessages;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectLists;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RecurringJobsHandler : IRecurringJobsHandler
    {
        public RecurringJobsHandler(IServiceProvider serviceProbider, HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler)
        {
            _serviceProbider = serviceProbider;
            _offHoursHandler = offHoursHandler;
        }

        private readonly IServiceProvider _serviceProbider;
        private readonly HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> _offHoursHandler;

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

        public async Task CreateAndPublishJobsByHalIdAsync(string halId)
        {
            // 1. Run a daily job that scans for any new supported time zones

            // 2. For each supported time zone create a recurring job called 'ActiveCampaigns_EasternStandardTime' or 'ActiveCampaigns_CentralStandardTime' if one does not already exist and schedule it

            // 3. Each recurring job for specific TimeZone is triggered, will get the timezoneId

            // 4. The recurring job will then look at the lookup table HalsTimeZones and retrieve all hal ids that match 'Eastern Standard Time' and trigger all of the required daily phases

            /////
            
            // 1. when new hal unit is onboarded, add that hal unit id to the look up table HalsTimeZones with the corresponding TimeZoneId            

        }
    }
}
