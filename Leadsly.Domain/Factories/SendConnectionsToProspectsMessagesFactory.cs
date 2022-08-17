using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class SendConnectionsToProspectsMessagesFactory : ISendConnectionsToProspectsMessagesFactory
    {
        public SendConnectionsToProspectsMessagesFactory(
            ILogger<SendConnectionsToProspectsMessagesFactory> logger,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _halRepository = halRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IHalRepository _halRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<SendConnectionsToProspectsMessagesFactory> _logger;

        public async Task<SendConnectionsBody> CreateMessageAsync(Campaign activeCampaign, CampaignWarmUp campaignWarmUp = null, CancellationToken ct = default)
        {
            return await CreateSendConnectionsBodyAsync(activeCampaign, campaignWarmUp, ct);
        }

        private async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(Campaign campaign, CampaignWarmUp campaignWarmUp = null, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            string halId = campaign.HalId;
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halUnit.HalId, ct);
            EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Grid);
            string virtualAssistantId = virtualAssistant.VirtualAssistantId;
            if (gridEcsService == null)
            {
                throw new Exception($"Grid ecs service not found for virtual assistant {virtualAssistantId}.");
            }

            if (gridEcsService.CloudMapDiscoveryService == null)
            {
                throw new Exception($"Cloud map discovery service not found for virtual assistant {virtualAssistantId}.");
            }

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
                _logger.LogDebug("Daily connection has been updated because warm up is enabled. Current daily warm up limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.SendConnectionRequests is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            SendConnectionsBody sendConnectionsBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                ChromeProfileName = chromeProfileName,
                DailyLimit = dailyConnectionsLimit,
                HalId = halUnit.HalId,
                UserId = campaign.ApplicationUserId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                StartDateTimestamp = campaign.StartTimestamp,
                CampaignId = campaign.CampaignId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("SendConnectionsBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is  {halId}", gridNamespaceName, halId);
            _logger.LogTrace("SendConnectionsBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is  {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("SendConnectionsBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is  {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("SendConnectionsBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is  {halId}", appServerServiceDiscoveryname, halId);

            return sendConnectionsBody;
        }

        public IList<SendConnectionsStageBody> CreateStages(IList<SendConnectionsStage> sendConnectionsStages, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            return GetSendConnectionsStages(sendConnectionsStages, dailyConnectionsLimit, ct);
        }

        private IList<SendConnectionsStageBody> GetSendConnectionsStages(IList<SendConnectionsStage> sendConnectionsStages, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            _logger.LogInformation("Getting the number of connections each stage should send to stay within the daily limit");
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = new List<SendConnectionsStageBody>();

            decimal totalInvites = dailyConnectionsLimit;
            int divider = sendConnectionsStages.Count;
            List<int> stagesConnectionsLimit = new();
            while (divider > 0)
            {
                decimal stageLimits = Math.Round(totalInvites / sendConnectionsStages.Count, 0);
                int currentStageLimits = Convert.ToInt32(stageLimits);
                stagesConnectionsLimit.Add(currentStageLimits);
                divider--;
            }

            int numOfStages = sendConnectionsStages.Count;
            _logger.LogDebug("Number of send connection stages for this campaign is {numOfStages}", numOfStages);
            for (int i = 0; i < numOfStages; i++)
            {
                int order = sendConnectionsStages[i].Order;
                int stageConnectionsLimit = stagesConnectionsLimit[i];

                SendConnectionsStageBody sendConnectionsStageBody = new()
                {
                    ConnectionsLimit = stageConnectionsLimit,
                    StartTime = sendConnectionsStages[i].StartTime,
                    Order = order
                };

                _logger.LogDebug("This send connections stage has the order of {order}. The limit is {stageConnectionsLimit}", order, stageConnectionsLimit);

                sendConnectionsStagesBody.Add(sendConnectionsStageBody);
            }

            return sendConnectionsStagesBody;
        }
    }
}
