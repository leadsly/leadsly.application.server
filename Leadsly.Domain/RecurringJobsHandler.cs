using Leadsly.Domain.Campaigns;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.UncontactedFollowUpMessages;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectLists;
using Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand> monitorHandler = 
                    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsAllCommand>>();

                MonitorForNewConnectionsAllCommand monitorForNewConnectionsAllCommand = new MonitorForNewConnectionsAllCommand();
                await monitorHandler.HandleAsync(monitorForNewConnectionsAllCommand);

                HalWorkCommandHandlerDecorator<ProspectListsCommand> prospectsHandler = 
                    scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<ProspectListsCommand>>();

                ProspectListsCommand prospectListsCommand = new ProspectListsCommand();
                await prospectsHandler.HandleAsync(prospectListsCommand);

                HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand> uncontactedHandler =
                   scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<UncontactedFollowUpMessageCommand>>();
                
                UncontactedFollowUpMessageCommand uncontactedCommand = new UncontactedFollowUpMessageCommand();
                await uncontactedHandler.HandleAsync(uncontactedCommand);

                HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepScanHandler =
                   scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand>>();

                DeepScanProspectsForRepliesCommand deepScanCommand = new DeepScanProspectsForRepliesCommand();
                await deepScanHandler.HandleAsync(deepScanCommand);
            } 
        }
    }
}
