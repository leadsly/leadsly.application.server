using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.UncontactedFollowUpMessages;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectLists;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RecurringJobsHandler : IRecurringJobsHandler
    {
        public RecurringJobsHandler(IServiceProvider serviceProbider)
        {
            _serviceProbider = serviceProbider;
        }

        private readonly IServiceProvider _serviceProbider;

        public async Task CreateAndPublishJobsAsync()
        {
            using (var scope = _serviceProbider.CreateScope())
            {
                ////////////////////////////////////////////////////////////////////////////////////
                /// ScanForNewConnectionsOffHours
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler =
                    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand>>();

                CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand();
                await offHoursHandler.HandleAsync(offHoursCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// MonitorForNewConnectionsAll
                ////////////////////////////////////////////////////////////////////////////////////
                //HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand> monitorHandler = 
                //    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand>>();

                //MonitorForNewConnectionsAllCommand monitorForNewConnectionsAllCommand = new MonitorForNewConnectionsAllCommand();
                //await monitorHandler.HandleAsync(monitorForNewConnectionsAllCommand);

                //////////////////////////////////////////////////////////////////////////////////////
                ///// UncontactedFollowUpMessageCommand
                //////////////////////////////////////////////////////////////////////////////////////
                //HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand> uncontactedHandler =
                //   scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand>>();

                //UncontactedFollowUpMessageCommand uncontactedCommand = new UncontactedFollowUpMessageCommand();
                //await uncontactedHandler.HandleAsync(uncontactedCommand);

                //IPhaseManager phaseManager = scope.ServiceProvider.GetRequiredService<IPhaseManager>();

                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///// Prospecting [ DeepScanProspectsForReplies OR (FollowUpMessagePhase AND ScanProspectsForReplies) ]
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                //await phaseManager.ProspectingPhaseAsync();

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ///// NetworkingConnectionsPhase [ ProspectListPhase OR SendConnectionsPhase ]
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                //await phaseManager.NetworkingConnectionsPhaseAsync();

            }
        }
    }
}
