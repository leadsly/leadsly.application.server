using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class NetworkingMessagesFactory : INetworkingMessagesFactory
    {
        public NetworkingMessagesFactory(
            ICampaignRepositoryFacade campaignRepositoryFacade, 
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider,
            ILogger<NetworkingMessagesFactory> logger)
        {
            _rabbitMQProvider = rabbitMQProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _logger = logger;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<NetworkingMessagesFactory> _logger;

        public async Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string halId, CancellationToken ct = default)
        {
            List<NetworkingMessageBody> messages = new List<NetworkingMessageBody>();
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            foreach (Campaign activeCampaign in campaigns)
            {
                // filter here based on searchurlprogress
                if (activeCampaign.SearchUrlsProgress.Any(s => s.Exhausted == false))
                {
                    IList<NetworkingMessageBody> msgs = await CreateNetworkingMessagesAsync(activeCampaign.CampaignId, activeCampaign.ApplicationUserId);
                    messages.AddRange(msgs);
                }
            }

            return messages;
        }

        public async Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            IList<NetworkingMessageBody> messages = new List<NetworkingMessageBody>();
            IList<IDictionary<int, string>> propsectsToCrawlAndTimes = await GetNumberOfNetworkingPhasesAsync(campaignId);
            foreach (var propsectsToCrawlAndTime in propsectsToCrawlAndTimes)
            {
                foreach (var item in propsectsToCrawlAndTime)
                {
                    int numberOfProspectsToCrawl = item.Key;
                    string startTime = item.Value;
                    NetworkingMessageBody message = await CreateNetworkingMessagesAsync(campaignId, userId, numberOfProspectsToCrawl, startTime, ct);
                    messages.Add(message);
                }
            }

            return messages;
        }

        private async Task<IList<IDictionary<int, string>>> GetNumberOfNetworkingPhasesAsync(string campaignId, CancellationToken ct = default)
        {
            IList<IDictionary<int, string>> propsectsToCrawlAndTime = new List<IDictionary<int, string>>();
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
            if (campaign.IsWarmUpEnabled == true)
            {

            }
            else
            {
                IList<SendConnectionsStage> sendConnectionsStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(campaignId, ct);
                int divider = sendConnectionsStages.Count;
                decimal totalInvites = campaign.DailyInvites;
                List<int> stagesConnectionsLimit = new();
                while (divider > 0)
                {
                    decimal stageLimits = Math.Round(totalInvites / sendConnectionsStages.Count, 0);
                    int currentStageLimits = Convert.ToInt32(stageLimits);
                    stagesConnectionsLimit.Add(currentStageLimits);
                    divider--;
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

        private async Task<NetworkingMessageBody> CreateNetworkingMessagesAsync(string campaignId, string userId, int numberOfProspectsToCrawl, string startTime, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
            PrimaryProspectList primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByIdAsync(campaign.CampaignProspectList.PrimaryProspectListId, ct);

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                CampaignWarmUp campaignWarmUp = await _campaignRepositoryFacade.GetCampaignWarmUpByIdAsync(campaignId, ct);
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
                _logger.LogDebug("Daily connection has been updated because warm up is enabled. Current daily warm up limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.Networking, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.Networking, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.SendConnectionRequests is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            NetworkingMessageBody sendConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,
                HalId = halUnit.HalId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                CampaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId,
                PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                StartTime = startTime,
                ProspectsToCrawl = numberOfProspectsToCrawl,
                EndOfWorkday = halUnit.EndHour,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("SendConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("SendConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return sendConnectionsBody;
        }
    }
}
