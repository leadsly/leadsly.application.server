using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.NetworkingHandler
{
    public class NetworkingCommandHandler : ICommandHandler<NetworkingCommand>
    {
        public NetworkingCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<NetworkingCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _timestampService = timestampService;
            _messageBrokerOutlet = messageBrokerOutlet;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<NetworkingCommandHandler> _logger;

        public async Task HandleAsync(NetworkingCommand command)
        {
            if (command.HalIds != null && command.HalIds.Count > 0)
            {
                await HandleInternalListAsync(command.HalIds);
            }
            // triggered on new campaign
            else if (command.CampaignId != null && command.UserId != null)
            {
                await OnNewCampaignAsync(command.CampaignId, command.UserId);
            }
        }

        private async Task OnNewCampaignAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            IList<NetworkingMessageBody> messages = await GetNumberOfNetworkingPhasesAsync(campaignId);
            // IList<NetworkingMessageBody> messages = await CreateNetworkingMessagesAsync(campaignId, userId);
            IList<SendConnectionsStageBody> stages = await CreateStagesAsync(messageBody);
            await SchedulePhaseMessagesAsync(messageBody, stages);
        }

        private async Task<IList<NetworkingMessageBody>> GetNumberOfNetworkingPhasesAsync(string campaignId, CancellationToken ct = default)
        {
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
            if(campaign.IsWarmUpEnabled == true)
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
                IList<NetworkingMessageBody> messageBodies = new List<NetworkingMessageBody>();

                for (int i = 0; i < stagesConnectionsLimit.Count; i++)
                {
                    int numberOfPhases = Math.DivRem(stagesConnectionsLimit[i], 10, out int remainder);
                    if(numberOfPhases == 0)
                    {
                        // this means stageConnLimit was smaller then or equal to 10
                        NetworkingMessageBody body = new()
                        {
                            
                        };
                    }
                    else
                    {
                        // numberOfPhases becomes the number of Networking phases we need to trigger for this stage
                    }
                }
            }
        }

        private async Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);

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
                DailyLimit = dailyConnectionsLimit,
                HalId = halUnit.HalId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
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
