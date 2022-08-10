using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class ScanProspectsForRepliesMessagesFactory : IScanProspectsForRepliesMessagesFactory
    {
        public ScanProspectsForRepliesMessagesFactory(
            ILogger<ScanProspectsForRepliesMessagesFactory> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            IUserProvider userProvider,
            ITimestampService timestampService)
        {
            _logger = logger;
            _userProvider = userProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _timestampService = timestampService;
        }

        private readonly IUserProvider _userProvider;
        private readonly ILogger<ScanProspectsForRepliesMessagesFactory> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;

        public async Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, IList<CampaignProspect> campaignProspects = default, CancellationToken ct = default)
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);

            SocialAccount socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
            ScanProspectsForRepliesBody messageBody = default;
            if (socialAccount.User.Campaigns.Any(c => c.Active == true))
            {
                // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;

                return await CreateScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halId, campaignProspects, ct);
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
        private async Task<ScanProspectsForRepliesBody> CreateScanProspectsForRepliesBodyAsync(string scanProspectsForRepliesPhaseId, string halId, IList<CampaignProspect> campaignProspects = default, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating scanprospects for replies body message for rabbit mq message broker.");
            ScanProspectsForRepliesPhase scanProspectsForRepliesPhase = await _campaignRepositoryFacade.GetScanProspectsForRepliesPhaseByIdAsync(scanProspectsForRepliesPhaseId, ct);

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);

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
                PageUrl = scanProspectsForRepliesPhase.PageUrl,
                HalId = halId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                ChromeProfileName = chromeProfileName,
                UserId = scanProspectsForRepliesPhase.SocialAccount.UserId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            // the section below attempts to determine what was the last follow up message that we have sent to the prospect and grab its content.
            // this is important because hal will then go through all of these proepcts and search messaging history and look for that specific message.
            if (campaignProspects != null)
            {
                IList<ContactedCampaignProspect> contactedCampaignProspects = new List<ContactedCampaignProspect>();
                foreach (CampaignProspect campaignProspect in campaignProspects)
                {
                    int lastFollowUpMessageOrder = campaignProspect.SentFollowUpMessageOrderNum;
                    CampaignProspectFollowUpMessage lastFollowUpMessage = campaignProspect.FollowUpMessages.Where(x => x.Order == lastFollowUpMessageOrder).FirstOrDefault();
                    if (lastFollowUpMessage == null)
                    {
                        string campaignProspectId = campaignProspect.CampaignProspectId;
                        int orderNum = campaignProspect.SentFollowUpMessageOrderNum;
                        _logger.LogWarning("Could not determine user's last sent follow up message. CampaignProspectId: {campaignProspectId}\r\n FollowUpMessageOrder: {orderNum}", campaignProspectId, orderNum);
                    }
                    else
                    {
                        ContactedCampaignProspect contactedCampaignProspect = new()
                        {
                            CampaignProspectId = campaignProspect.CampaignProspectId,
                            LastFollowUpMessageContent = lastFollowUpMessage.Content,
                            Name = campaignProspect.Name,
                            ProspectProfileUrl = campaignProspect.ProfileUrl
                        };

                        contactedCampaignProspects.Add(contactedCampaignProspect);
                    }
                }
                scanProspectsForRepliesBody.ContactedCampaignProspects = contactedCampaignProspects;
            }

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("ProspectListBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("ProspectListBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return scanProspectsForRepliesBody;
        }
    }
}
