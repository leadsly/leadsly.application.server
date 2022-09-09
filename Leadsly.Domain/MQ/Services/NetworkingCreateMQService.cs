using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class NetworkingCreateMQService : INetworkingCreateMQService
    {
        public NetworkingCreateMQService(
            ILogger<NetworkingCreateMQService> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            INetworkingMessagesFactory factory
            )
        {
            _factory = factory;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _logger = logger;
        }

        private readonly INetworkingMessagesFactory _factory;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ILogger<NetworkingCreateMQService> _logger;

        public async Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesAsync(string halId, CancellationToken ct = default)
        {
            List<PublishMessageBody> mqMessagesOut = new List<PublishMessageBody>();
            IList<Campaign> activeCampaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            foreach (Campaign activeCampaign in activeCampaigns)
            {
                IList<PublishMessageBody> mqMessages = await CreateMQNetworkingMessagesForCampaignAsync(halId, activeCampaign, ct);
                if (mqMessages != null)
                {
                    mqMessagesOut.AddRange(mqMessages);
                }
            }

            return mqMessagesOut;
        }

        public async Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesAsync(string halId, Campaign campaign, CancellationToken ct = default)
        {
            List<PublishMessageBody> mqMessagesOut = new List<PublishMessageBody>();

            IList<PublishMessageBody> mqMessages = await CreateMQNetworkingMessagesForCampaignAsync(halId, campaign, ct);
            if (mqMessages != null)
            {
                mqMessagesOut.AddRange(mqMessages);
            }

            return mqMessagesOut;
        }

        private async Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesForCampaignAsync(string halId, Campaign activeCampaign, CancellationToken ct = default)
        {
            List<PublishMessageBody> mqMessagesOut = new List<PublishMessageBody>();
            PrimaryProspectList primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByIdAsync(activeCampaign.CampaignProspectList.PrimaryProspectListId, ct);
            IList<IDictionary<int, string>> propsectsToCrawlAndTimes = await GetNumberOfNetworkingPhasesAsync(activeCampaign);
            foreach (IDictionary<int, string> propsectsToCrawlAndTime in propsectsToCrawlAndTimes)
            {
                IList<PublishMessageBody> mqMessages = await CreateMQNetworkingMessagesForProspectsAsync(halId, activeCampaign, primaryProspectList, propsectsToCrawlAndTime, ct = default);
                if (mqMessages != null)
                {
                    mqMessagesOut.AddRange(mqMessages);
                }
            }

            return mqMessagesOut;
        }

        private async Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesForProspectsAsync(string halId, Campaign campaign, PrimaryProspectList primaryProspectList, IDictionary<int, string> propsectsToCrawlAndTime, CancellationToken ct = default)
        {
            List<PublishMessageBody> mqMessages = new List<PublishMessageBody>();
            foreach (KeyValuePair<int, string> prospect in propsectsToCrawlAndTime)
            {
                int numberOfProspectsToCrawl = prospect.Key;
                string startTime = prospect.Value;
                PublishMessageBody mqMessage = await _factory.CreateMQMessageAsync(halId, startTime, numberOfProspectsToCrawl, campaign, primaryProspectList, ct);
                if (mqMessage != null)
                {
                    mqMessages.Add(mqMessage);
                }
            }
            return mqMessages;
        }

        private async Task<IList<IDictionary<int, string>>> GetNumberOfNetworkingPhasesAsync(Campaign activeCampaign, CancellationToken ct = default)
        {
            _logger.LogDebug("Determining number of networking phases for campaign {campaignId}. This will return number of prospects we should connect with and what time.", activeCampaign.CampaignId);
            IList<IDictionary<int, string>> propsectsToCrawlAndTime = new List<IDictionary<int, string>>();

            if (activeCampaign.IsWarmUpEnabled == true)
            {
                _logger.LogWarning("Campaign was configured to use warm up, but warm up has not been implemented");
            }
            else
            {
                IList<SendConnectionsStage> sendConnectionsStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(activeCampaign.CampaignId, ct);
                _logger.LogDebug($"Number of SendConnectionsStages is: {sendConnectionsStages.Count}");

                int invitesPerStage = Math.DivRem(activeCampaign.DailyInvites, sendConnectionsStages.Count, out int remainderInvites);
                _logger.LogDebug("Number of invites that should happen per stage is {invitesPerStage}", invitesPerStage);
                _logger.LogTrace("The remainder of invites is {remainderInvites}", remainderInvites);

                List<int> stagesConnectionsLimit = sendConnectionsStages.OrderBy(s => s.Order).Select(_ => invitesPerStage).ToList();
                if (remainderInvites != 0)
                {
                    int last = stagesConnectionsLimit.Last();
                    stagesConnectionsLimit.RemoveAt(stagesConnectionsLimit.Count - 1);
                    last += remainderInvites;
                    stagesConnectionsLimit.Add(last);
                }

                for (int i = 0; i < stagesConnectionsLimit.Count; i++)
                {
                    int numberOfPhases = Math.DivRem(stagesConnectionsLimit[i], 10, out int remainder);
                    // numberOfPhases becomes the number of Networking phases we need to trigger for this stage
                    for (int j = 0; j < numberOfPhases; j++)
                    {
                        propsectsToCrawlAndTime.Add(
                                new Dictionary<int, string>()
                                {
                                    { 10, sendConnectionsStages[i].StartTime }
                                }
                            );
                    }
                    if (remainder != 0)
                    {
                        propsectsToCrawlAndTime.Add(
                                new Dictionary<int, string>()
                                {
                                    { remainder, sendConnectionsStages[i].StartTime }
                                }
                            );
                    }
                }
            }

            return propsectsToCrawlAndTime;
        }
    }
}
