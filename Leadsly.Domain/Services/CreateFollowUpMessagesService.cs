using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CreateFollowUpMessagesService : ICreateFollowUpMessagesService
    {
        public CreateFollowUpMessagesService(
            ILogger<CreateFollowUpMessagesService> logger,
            ICreateFollowUpMessageService service,
            IMemoryCache memoryCache,
            ICampaignRepositoryFacade campaignFacadeRepository)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _service = service;
            _campaignFacadeRepository = campaignFacadeRepository;
        }

        private IMemoryCache _memoryCache;
        private ILogger<CreateFollowUpMessagesService> _logger;
        private ICreateFollowUpMessageService _service;
        private ICampaignRepositoryFacade _campaignFacadeRepository;

        public async Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectsFollowUpMessagesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogDebug("Retrieving active campaigns for halId {halId}", halId);
            IList<Campaign> activeCampaigns = await GetCampaignsAsync(halId);
            _logger.LogDebug("Retrieved {activeCampaignsCount} active campaigns for halId {halId}", activeCampaigns?.Count, halId);

            IList<CampaignProspectFollowUpMessage> prospectsFollowUpMessages = await GenerateFollowUpMessagesAsync(halId, activeCampaigns, ct);
            if (prospectsFollowUpMessages.Count == 0)
            {
                _logger.LogDebug("No follow up messages generated for halId {halId}", halId);
            }
            else
            {
                _logger.LogDebug("Generated {prospectsFollowUpMessagesCount} follow up messages for halId {halId}", prospectsFollowUpMessages.Count, halId);
            }

            return prospectsFollowUpMessages;
        }

        private async Task<IList<Campaign>> GetCampaignsAsync(string halId)
        {
            if (_memoryCache.TryGetValue(halId, out IList<Campaign> campaigns) == false)
            {
                campaigns = await _campaignFacadeRepository.GetAllActiveCampaignsByHalIdAsync(halId);
                _memoryCache.Set(halId, campaigns);
            }
            return campaigns;
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> GenerateFollowUpMessagesAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default)
        {
            List<CampaignProspectFollowUpMessage> prospectsFollowUpMessages = new List<CampaignProspectFollowUpMessage>();
            foreach (Campaign activeCampaign in campaigns)
            {
                // grab all campaign prospects for each campaign
                IList<CampaignProspect> eligibleProspects = await _campaignFacadeRepository.GetAllFollowUpMessageEligbleProspectsByCampaignIdAsync(activeCampaign.CampaignId, ct);
                if (eligibleProspects.Count > 0)
                {
                    _logger.LogDebug("Campaign with id {0}, has {1} eligible prospects for follow up messages", activeCampaign.CampaignId, eligibleProspects.Count);
                    prospectsFollowUpMessages.AddRange(await CreateProspectFollowUpMessagesAsync(halId, eligibleProspects, ct));
                }
                else
                {
                    _logger.LogInformation("Campaign with id {0}, does not have any eligible prospects for follow up messages yet.", activeCampaign.CampaignId);
                }
            }

            return prospectsFollowUpMessages;
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> CreateProspectFollowUpMessagesAsync(string halId, IList<CampaignProspect> eligibleProspects, CancellationToken ct = default)
        {
            List<CampaignProspectFollowUpMessage> prospectMessages = new List<CampaignProspectFollowUpMessage>();
            foreach (CampaignProspect eligibleProspect in eligibleProspects)
            {
                IList<FollowUpMessage> followUpMessages = await GetFollowUpMessagesByCampaignIdAsync(eligibleProspect.CampaignId, ct);
                IList<CampaignProspectFollowUpMessage> prospectFollowUpMessages = await _service.GenerateProspectFollowUpMessagesAsync(halId, eligibleProspect, followUpMessages, ct);
                if (prospectFollowUpMessages != null)
                {
                    prospectMessages.AddRange(prospectFollowUpMessages);
                }
            }

            return prospectMessages;
        }

        private async Task<IList<FollowUpMessage>> GetFollowUpMessagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(campaignId, out IList<FollowUpMessage> followUpMessages) == false)
            {
                followUpMessages = await _campaignFacadeRepository.GetFollowUpMessagesByCampaignIdAsync(campaignId, ct);
                _memoryCache.Set(campaignId, followUpMessages);
            }
            return followUpMessages;
        }
    }
}
