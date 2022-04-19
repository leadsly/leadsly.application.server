using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignPhaseClient : ICampaignPhaseClient
    {
        public CampaignPhaseClient(ICampaignPhaseCommandProducer commandProducer, ICampaignManager campaignManager,
            ISendFollowUpMessageProvider sendFollowUpMessageProvider)
        {
            _commandProducer = commandProducer;
            _campaignManager = campaignManager;
            _sendFollowUpMessageProvider = sendFollowUpMessageProvider;
        }

        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;
        private readonly ICampaignPhaseCommandProducer _commandProducer;
        private readonly ICampaignManager _campaignManager;
        public async Task HandleNewCampaignAsync(Campaign campaign)
        {
            // ensure ScanForProspectReplies, ConnectionWithdraw and MonitorForNewProspects phases are running on hal
            // always trigger them here
            IList<ICommand> commands = new List<ICommand>();
            ICommand monitorCommand = _commandProducer.CreateMonitorForNewProspectsCommand(campaign.HalId, campaign.ApplicationUserId);
            ICommand scanCommand = _commandProducer.CreateScanProspectsForRepliesCommand(campaign.HalId, campaign.ApplicationUserId);

            // if prospect list phase does not exists, this means were running campaign off of existing prospect list
            if (campaign.ProspectListPhase == null)
            {
                // await _campaignManager.TriggerSendConnectionsPhaseAsync(campaign.CampaignId, campaign.ApplicationUserId);
                ICommand sendCommand = _commandProducer.CreateSendConnectionsCommand(campaign.CampaignId, campaign.ApplicationUserId);
                commands.Add(sendCommand);
            }
            else
            {
                // await _campaignManager.TriggerProspectListPhaseAsync(campaign.ProspectListPhase.ProspectListPhaseId, campaign.ApplicationUserId);
                ICommand prospectListCommand = _commandProducer.CreateProspectListCommand(campaign.ProspectListPhase.ProspectListPhaseId, campaign.ApplicationUserId);
                commands.Add(prospectListCommand);
            }

            commands.Add(monitorCommand);
            commands.Add(scanCommand);

            _campaignManager.SetCommands(commands);
            await _campaignManager.ExecuteAllAsync();
        }

        public async Task ProduceSendConnectionsPhaseAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            ICommand command = _commandProducer.CreateSendConnectionsCommand(campaignId, userId);
            _campaignManager.SetCommand(command);
            await _campaignManager.ExecuteAsync();
        }

        public async Task ProduceScanProspectsForRepliesPhaseAsync(string halId, string userId, CancellationToken ct = default)
        {
            ICommand command = _commandProducer.CreateScanProspectsForRepliesCommand(halId, userId);
            _campaignManager.SetCommand(command);
            await _campaignManager.ExecuteAsync();
        }

        public async Task ProduceFollowUpMessagesPhaseAsync(string halId, string userId, CancellationToken ct = default)
        {
            ICommand command = _commandProducer.CreateFollowUpMessagesCommand(halId);
            _campaignManager.SetCommand(command);
            await _campaignManager.ExecuteAsync();
        }

        public async Task ProduceSendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            var messagesToGoingOut = await _sendFollowUpMessageProvider.CreateSendFollowUpMessagesAsync(campaignProspects, ct);

            IList<ICommand> commands = new List<ICommand>();
            foreach (var messagePair in messagesToGoingOut)
            {
                ICommand command = _commandProducer.CreateFollowUpMessageCommand(messagePair.Key.CampaignProspectFollowUpMessageId, messagePair.Key.CampaignProspect.CampaignId, messagePair.Value);
                commands.Add(command);
            }

            _campaignManager.SetCommands(commands);
            await _campaignManager.ExecuteAllAsync();
        }
    }
}
