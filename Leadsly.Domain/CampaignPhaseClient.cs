using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Campaigns;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessage;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectList;
using Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers;
using Leadsly.Domain.Campaigns.SendConnectionsToProspectsHandlers;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class CampaignPhaseClient : ICampaignPhaseClient
    {
        public CampaignPhaseClient(
            ICommandHandler<ScanProspectsForRepliesCommand> scanHandler,
            HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> monitorHandler,
            HalWorkCommandHandlerDecorator<SendConnectionsToProspectsCommand> connectionsHandler,
            HalWorkCommandHandlerDecorator<ProspectListCommand> prospectListHandler,
            HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> followUpMessagesHandler,
            HalWorkCommandHandlerDecorator<FollowUpMessageCommand> followUpMessageHandler
            )
        {
            _scanHandler = scanHandler;
            _monitorHandler = monitorHandler;
            _connectionsHandler = connectionsHandler;
            _followUpMessagesHandler = followUpMessagesHandler;
            _followUpMessageHandler = followUpMessageHandler;
            _prospectListHandler = prospectListHandler;
        }

        private readonly HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> _followUpMessagesHandler;
        private readonly HalWorkCommandHandlerDecorator<FollowUpMessageCommand> _followUpMessageHandler;
        private readonly ICommandHandler<ScanProspectsForRepliesCommand> _scanHandler;
        private readonly HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> _monitorHandler;
        private readonly HalWorkCommandHandlerDecorator<SendConnectionsToProspectsCommand> _connectionsHandler;
        private readonly HalWorkCommandHandlerDecorator<ProspectListCommand> _prospectListHandler;
        public async Task HandleNewCampaignAsync(Campaign campaign)
        {
            // ensure ScanForProspectReplies, ConnectionWithdraw and MonitorForNewProspects phases are running on hal
            // always trigger them here            
            MonitorForNewConnectionsCommand monitorCommand = new MonitorForNewConnectionsCommand(campaign.HalId, campaign.ApplicationUserId);
            await _monitorHandler.HandleAsync(monitorCommand);

            ScanProspectsForRepliesCommand scanCommand = new ScanProspectsForRepliesCommand(campaign.HalId, campaign.ApplicationUserId);
            await _scanHandler.HandleAsync(scanCommand);

            // if prospect list phase does not exists, this means were running campaign off of existing prospect list
            if (campaign.ProspectListPhase == null)
            {
                SendConnectionsToProspectsCommand sendConnections = new SendConnectionsToProspectsCommand(campaign.CampaignId, campaign.ApplicationUserId);
                await _connectionsHandler.HandleAsync(sendConnections);
            }
            else
            {
                ProspectListCommand prospectListCommand = new ProspectListCommand(campaign.ProspectListPhase.ProspectListPhaseId, campaign.ApplicationUserId);
                await _prospectListHandler.HandleAsync(prospectListCommand);
            }
        }

        public async Task ProduceSendConnectionsPhaseAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            SendConnectionsToProspectsCommand sendConnections = new SendConnectionsToProspectsCommand(campaignId, userId);
            await _connectionsHandler.HandleAsync(sendConnections);
        }

        public async Task ProduceScanProspectsForRepliesPhaseAsync(string halId, string userId, CancellationToken ct = default)
        {
            ScanProspectsForRepliesCommand scanCommand = new ScanProspectsForRepliesCommand(halId, userId);
            await _scanHandler.HandleAsync(scanCommand);
        }

        public async Task ProduceFollowUpMessagesPhaseAsync(string halId, string userId, CancellationToken ct = default)
        {
            FollowUpMessagesCommand followUpCommand = new FollowUpMessagesCommand(halId);
            await _followUpMessagesHandler.HandleAsync(followUpCommand);            
        }

        public async Task ProduceSendFollowUpMessagesAsync(IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut, CancellationToken ct = default)
        {
            foreach (var messagePair in messagesGoingOut)
            {
                string campaignId = messagePair.Key.CampaignProspect.CampaignId;
                string messageId = messagePair.Key.CampaignProspectFollowUpMessageId;
                DateTimeOffset scheduleTime = messagePair.Value;

                FollowUpMessageCommand followUpCommand = new FollowUpMessageCommand(campaignId, messageId, scheduleTime);
                await _followUpMessageHandler.HandleAsync(followUpCommand);
            }
        }
    }
}
