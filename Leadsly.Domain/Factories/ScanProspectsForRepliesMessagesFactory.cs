using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChromeProfile = Leadsly.Domain.Models.Entities.ChromeProfile;
using PhaseType = Leadsly.Domain.Models.Entities.Campaigns.Phases.PhaseType;
using ScanProspectsForRepliesPhase = Leadsly.Domain.Models.Entities.Campaigns.Phases.ScanProspectsForRepliesPhase;

namespace Leadsly.Domain.Factories
{
    public class ScanProspectsForRepliesMessagesFactory : IScanProspectsForRepliesMessagesFactory
    {
        public ScanProspectsForRepliesMessagesFactory(
            ILogger<ScanProspectsForRepliesMessagesFactory> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            IUserProvider userProvider,
            ICloudPlatformRepository cloudPlatformRepository,
            ITimestampService timestampService)
        {
            _logger = logger;
            _userProvider = userProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
            _virtualAssistantRepository = virtualAssistantRepository;
            _timestampService = timestampService;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IUserProvider _userProvider;
        private readonly ILogger<ScanProspectsForRepliesMessagesFactory> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;

        public async Task<DeepScanProspectsForRepliesBody> CreateDeepScanMessageAsync(string halId, CancellationToken ct = default)
        {
            DeepScanProspectsForRepliesBody messageBody = default;
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);
            if (halUnit != null)
            {
                _logger.LogInformation("HalUnit with HalId {halId} found.", halId);
                SocialAccount socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
                if (socialAccount != null)
                {
                    string email = socialAccount.Username;
                    _logger.LogDebug("Social account {email} is associated with HalId {halId}", email, halId);

                    if (socialAccount.User?.Campaigns?.Any(c => c.Active == true) == true)
                    {
                        _logger.LogDebug("Active campaigns have been found for halId {halId}", halId);
                        // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                        string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;

                        return await CreateDeepScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halId, ct);
                    }
                    else
                    {
                        _logger.LogDebug("No active campaigns were found for halId {halId}", halId);
                    }
                }
            }
            else
            {
                _logger.LogDebug("HalUnit with HalId {halId} not found.", halId);
            }

            return messageBody;
        }

        public async Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, VirtualAssistant virtualAssistant, ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            if (halUnit == null)
            {
                _logger.LogDebug("Failed to get hal unit by halId {halId}", halId);
                return null;
            }

            _logger.LogInformation("Creating scanprospects for replies body message for rabbit mq message broker.");
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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ScanForReplies, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                ChromeProfile profileName = new()
                {
                    CampaignPhaseType = PhaseType.FollwUpMessage,
                    Name = Guid.NewGuid().ToString()
                };
                await _halRepository.CreateChromeProfileAsync(profileName, ct);
                chromeProfileName = profileName.Name;
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ScanForReplies is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            ScanProspectsForRepliesBody scanProspectsForRepliesBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                PageUrl = phase.PageUrl,
                HalId = halUnit.HalId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                ChromeProfileName = chromeProfileName,
                UserId = userId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is: {halId}", gridNamespaceName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is  {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is  {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is  {halId}", appServerServiceDiscoveryname, halId);

            return scanProspectsForRepliesBody;
        }

        public async Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, CancellationToken ct = default)
        {
            ScanProspectsForRepliesBody messageBody = default;
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);
            if (halUnit != null)
            {
                _logger.LogInformation("HalUnit with HalId {halId} found.", halId);
                SocialAccount socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
                if (socialAccount != null)
                {
                    string email = socialAccount.Username;
                    _logger.LogDebug("Social account {email} is associated with HalId {halId}", email, halId);

                    if (socialAccount.User?.Campaigns?.Any(c => c.Active == true) == true)
                    {
                        _logger.LogDebug("Active campaigns have been found for halId {halId}", halId);
                        // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                        string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;

                        return await CreateScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halId, ct);
                    }
                    else
                    {
                        _logger.LogDebug("No active campaigns were found for halId {halId}", halId);
                    }
                }
            }
            else
            {
                _logger.LogDebug("HalUnit with HalId {halId} not found.", halId);
            }

            return messageBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanProspectsForRepliesPhaseId">Used by the DeepScanProspectsForRepliesPhase</param>
        /// <param name="halId"></param>
        /// <param name="contactedCampaignProspects"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScanProspectsForRepliesBody> CreateScanProspectsForRepliesBodyAsync(string scanProspectsForRepliesPhaseId, string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating scanprospects for replies body message for rabbit mq message broker.");
            ScanProspectsForRepliesPhase scanProspectsForRepliesPhase = await _campaignRepositoryFacade.GetScanProspectsForRepliesPhaseByIdAsync(scanProspectsForRepliesPhaseId, ct);

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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ScanForReplies, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.ScanForReplies, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ScanForReplies is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            ScanProspectsForRepliesBody scanProspectsForRepliesBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                PageUrl = scanProspectsForRepliesPhase.PageUrl,
                HalId = halId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                ChromeProfileName = chromeProfileName,
                UserId = scanProspectsForRepliesPhase.SocialAccount.UserId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is: {halId}", gridNamespaceName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is  {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is  {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("ScanProspectsForRepliesBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is  {halId}", appServerServiceDiscoveryname, halId);

            return scanProspectsForRepliesBody;
        }

        private async Task<DeepScanProspectsForRepliesBody> CreateDeepScanProspectsForRepliesBodyAsync(string scanProspectsForRepliesPhaseId, string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating scanprospects for replies body message for rabbit mq message broker.");
            ScanProspectsForRepliesPhase scanProspectsForRepliesPhase = await _campaignRepositoryFacade.GetScanProspectsForRepliesPhaseByIdAsync(scanProspectsForRepliesPhaseId, ct);

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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ScanForReplies, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.ScanForReplies, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ScanForReplies is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            DeepScanProspectsForRepliesBody deepScanProspectsForRepliesBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                PageUrl = scanProspectsForRepliesPhase.PageUrl,
                HalId = halId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                ChromeProfileName = chromeProfileName,
                UserId = scanProspectsForRepliesPhase.SocialAccount.UserId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is: {halId}", gridNamespaceName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is  {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is  {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is  {halId}", appServerServiceDiscoveryname, halId);

            return deepScanProspectsForRepliesBody;
        }

    }
}
