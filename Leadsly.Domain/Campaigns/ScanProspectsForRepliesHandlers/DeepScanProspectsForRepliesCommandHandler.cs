using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class DeepScanProspectsForRepliesCommandHandler : ScanProspectsForRepliesCommandHandlerBase, ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider,            
            ITimestampService timestampService,
            ICampaignRepositoryFacade campaignRepositoryFacade
            ) : base(logger, campaignRepositoryFacade, rabbitMQProvider, halRepository, timestampService)
        {
            _messageBrokerOutlet = messageBrokerOutlet;            
            _logger = logger;
            _halRepository = halRepository;            
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
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
            await InternalExecuteListAsync(command);
        }

        private async Task InternalExecuteListAsync(DeepScanProspectsForRepliesCommand command)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = await CreateHalsCampainProspectsAsync(command);

            foreach (var halCampaignProspects in halsCampaignProspects)
            {
                HalUnit halUnit = await _halRepository.GetByHalIdAsync(halCampaignProspects.Key);
                string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
                string userId = halUnit.SocialAccount.UserId;
                string halId = halUnit.HalId;

                // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                ScanProspectsForRepliesBody messageBody = await CreateScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halId, halCampaignProspects.Value);

                InternalExecute(messageBody);
            }
        }

        private void InternalExecute(ScanProspectsForRepliesBody messageBody)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = messageBody.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<IDictionary<string, IList<CampaignProspect>>> CreateHalsCampainProspectsAsync(DeepScanProspectsForRepliesCommand command)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();

            foreach (string halId in command.HalIds)
            {
                // get all campaign prospects by halId
                IList<CampaignProspect> halCampaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
                IList<CampaignProspect> contactedProspects = halCampaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false).ToList();
                if (contactedProspects.Count > 0)
                {
                    halsCampaignProspects.Add(halId, contactedProspects);
                }
            }

            return halsCampaignProspects;
        }
    }
}
