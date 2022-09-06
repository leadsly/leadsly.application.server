using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class FollowUpMessagesService : IFollowUpMessagesService
    {
        public FollowUpMessagesService(
            ICreateFollowUpMessagesService service,
            ICampaignRepositoryFacade facade,
            IFollowUpMessagesFactory factory,
            ILogger<FollowUpMessagesService> logger)
        {
            _service = service;
            _facade = facade;
            _factory = factory;
            _logger = logger;
        }

        private readonly ICreateFollowUpMessagesService _service;
        private readonly ICampaignRepositoryFacade _facade;
        private readonly IFollowUpMessagesFactory _factory;
        private ILogger<FollowUpMessagesService> _logger;

        public async Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqFollowUpMessages = new List<PublishMessageBody>();
            IList<CampaignProspectFollowUpMessage> followUpMessages = await CreateFollowUpMessagesAsync(halId, ct);
            foreach (CampaignProspectFollowUpMessage followUpMessage in followUpMessages)
            {
                // the phase is only used for getting the url
                FollowUpMessagePhase phase = await _facade.GetFollowUpMessagePhaseByCampaignIdAsync(followUpMessage.CampaignProspect.CampaignId, ct);
                PublishMessageBody mqMessage = await _factory.CreateMQMessageAsync(halId, followUpMessage, phase, ct);
                if (mqMessage != null)
                {
                    mqFollowUpMessages.Add(mqMessage);
                }
            }

            return mqFollowUpMessages;
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> CreateFollowUpMessagesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation($"Getting follow up messages for hal {halId}");

            IList<CampaignProspectFollowUpMessage> prospectsFollowUpMessages = await _service.GenerateProspectsFollowUpMessagesAsync(halId, ct);

            foreach (CampaignProspectFollowUpMessage followUpMessage in prospectsFollowUpMessages)
            {
                string content = TokenizeMessage(followUpMessage.Content, followUpMessage.CampaignProspect);
                followUpMessage.Content = content;

                await _facade.CreateFollowUpMessageAsync(followUpMessage);
            }

            return prospectsFollowUpMessages;
        }

        private string TokenizeMessage(string message, CampaignProspect prospect)
        {
            string firstName = prospect.Name.Split(' ').FirstOrDefault();
            firstName = string.IsNullOrEmpty(firstName) ? "there" : firstName.Capitalize();
            string content = message.Replace("{firstName}", firstName);

            return content;
        }
    }
}
