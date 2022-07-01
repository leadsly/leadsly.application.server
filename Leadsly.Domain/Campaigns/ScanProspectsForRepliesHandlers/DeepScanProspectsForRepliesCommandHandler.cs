using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class DeepScanProspectsForRepliesCommandHandler : ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            IScanProspectsForRepliesMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade
            )
        {
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;            
            _logger = logger;         
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IScanProspectsForRepliesMessagesFactory _messagesFactory;
        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;        

        /// <summary>
        /// Triggered on recurring basis once a day. The purpose of this phase is to perform a deep analysis of the conversation history with any campaign prospect to who meets the following conditions
        /// Has accepted our connection request, has gotten a follow up message and has NOT yet replied to our message. This ensures that we can campture responses from campaign prospects
        /// even if leadsly user has communicated with the prospect themselves. Once Hal is finished with this phase, it will send a request to the application server to first trigger 
        /// FollowUpMessagePhase and then ScanProspectsForRepliesPhase
        /// </summary>
        /// <returns></returns>
        public async Task HandleAsync(DeepScanProspectsForRepliesCommand command)
        {
            if(command.HalIds != null)
            {
                await InternalExecuteAsync(command.HalIds);
            }
            else if(string.IsNullOrEmpty(command.HalId) == false)
            {
                await InternalExecuteAsync(command.HalId);
            }            
        }

        private async Task InternalExecuteAsync(string halId)
        {
            IList<string> halIds = new List<string>()
            {
                halId
            };

            await InternalExecuteAsync(halIds);
        }

        private async Task InternalExecuteAsync(IList<string> halIds)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = await CreateHalsCampainProspectsAsync(halIds);

            foreach (var halCampaignProspects in halsCampaignProspects)
            {
                string halId = halCampaignProspects.Key;

                // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                ScanProspectsForRepliesBody messageBody = await _messagesFactory.CreateMessageAsync(halId, halCampaignProspects.Value);

                PublishMessage(messageBody);
            }
        }

        private void PublishMessage(ScanProspectsForRepliesBody messageBody)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = messageBody.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<IDictionary<string, IList<CampaignProspect>>> CreateHalsCampainProspectsAsync(IList<string> halIds)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();

            foreach (string halId in halIds)
            {
                IList<CampaignProspect> halsCampProspects = await GetCampainProspectsAsync(halId);
                if(halsCampProspects.Count > 0)
                {
                    halsCampaignProspects.Add(halId, halsCampProspects);
                }
            }

            return halsCampaignProspects;
        }

        private async Task<IList<CampaignProspect>> GetCampainProspectsAsync(string halId)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();

            IList<CampaignProspect> halCampaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            IList<CampaignProspect> contactedProspects = halCampaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false).ToList();
            
            return contactedProspects;
        }
    }
}
