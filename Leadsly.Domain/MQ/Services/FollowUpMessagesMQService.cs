using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class FollowUpMessagesMQService : IFollowUpMessagesMQService
    {
        public FollowUpMessagesMQService(
            IFollowUpMessagesCreateMQService service,
            ICampaignRepositoryFacade facade,
            IFollowUpMessagesFactory factory,
            ILogger<FollowUpMessagesMQService> logger)
        {
            _service = service;
            _facade = facade;
            _factory = factory;
            _logger = logger;
        }

        private readonly IFollowUpMessagesCreateMQService _service;
        private readonly ICampaignRepositoryFacade _facade;
        private readonly IFollowUpMessagesFactory _factory;
        private ILogger<FollowUpMessagesMQService> _logger;

        public async Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, CancellationToken ct = default)
        {
            IList<CampaignProspectFollowUpMessage> followUpMessages = await CreateFollowUpMessagesAsync(halId, ct);
            return await CreateMQFollowUpMessagesAsync(halId, followUpMessages, ct);
        }

        public async Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default)
        {
            IList<CampaignProspectFollowUpMessage> followUpMessages = await CreateFollowUpMessagesAsync(halId, campaigns, ct);
            return await CreateMQFollowUpMessagesAsync(halId, followUpMessages, ct);
        }

        private async Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, IList<CampaignProspectFollowUpMessage> followUpMessages, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqFollowUpMessages = new List<PublishMessageBody>();
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

            // await CreateFollowUpMessagesAsync(prospectsFollowUpMessages);

            return prospectsFollowUpMessages;
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> CreateFollowUpMessagesAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default)
        {
            _logger.LogInformation($"Getting follow up messages for hal {halId}");

            IList<CampaignProspectFollowUpMessage> prospectsFollowUpMessages = await _service.GenerateProspectsFollowUpMessagesAsync(halId, campaigns, ct);

            // await CreateFollowUpMessagesAsync(prospectsFollowUpMessages);

            return prospectsFollowUpMessages;
        }

        private async Task CreateFollowUpMessagesAsync(IList<CampaignProspectFollowUpMessage> prospectsFollowUpMessages, CancellationToken ct = default)
        {
            foreach (CampaignProspectFollowUpMessage followUpMessage in prospectsFollowUpMessages)
            {
                string content = TokenizeMessage(followUpMessage.Content, followUpMessage.CampaignProspect);
                followUpMessage.Content = content;

                await _facade.CreateFollowUpMessageAsync(followUpMessage);
            }
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
