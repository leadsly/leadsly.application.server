using Leadsly.Application.Model;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class ProspectingJobService : IProspectingJobService
    {
        public ProspectingJobService(
            ILogger<ProspectingJobService> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IScanProspectsForRepliesMQCreator scanProspectsForRepliesMQCreator,
            IFollowUpMessagesMQCreator followUpMessagesMQCreator,
            IDeepScanProspectsForRepliesMQCreator deepScanProspectsForRepliesMQCreator,
            IMemoryCache memoryCache)
        {
            _followUpMessagesMQCreator = followUpMessagesMQCreator;
            _scanProspectsForRepliesMQCreator = scanProspectsForRepliesMQCreator;
            _memoryCache = memoryCache;
            _logger = logger;
            _deepScanProspectsForRepliesMQCreator = deepScanProspectsForRepliesMQCreator;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly ILogger<ProspectingJobService> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IFollowUpMessagesMQCreator _followUpMessagesMQCreator;
        private readonly IScanProspectsForRepliesMQCreator _scanProspectsForRepliesMQCreator;
        private readonly IDeepScanProspectsForRepliesMQCreator _deepScanProspectsForRepliesMQCreator;
        private readonly IMemoryCache _memoryCache;

        public async Task PublishProspectingMQMessagesAsync(string halId)
        {
            if (await ShouldPublishDeepScanAsync(halId) == true)
            {
                await _deepScanProspectsForRepliesMQCreator.PublishMessageAsync(halId);
            }
            else
            {
                await _followUpMessagesMQCreator.PublishMessageAsync(halId);

                await _scanProspectsForRepliesMQCreator.PublishMessageAsync(halId);
            }
        }

        public async Task PublishFollowUpMQMessagesAsync(string halId)
        {
            await _followUpMessagesMQCreator.PublishMessageAsync(halId);
        }

        /// <summary>
        /// If any of the prospects associated with HalUnit have accepted the connection invite, have gotten a follow up message and have NOT been marked as complete (this happens when all follow up messages go out AND not contact is made within certain time frame)
        /// we then proceed to execute DeepScanProspectsForReplies phase.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<bool> ShouldPublishDeepScanAsync(string halId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetAllCampaignProspectsByHalIdAsync(halId, ct);
            if (campaignProspects.Any(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false) == true)
            {
                return true;
            }
            return false;
        }

        private async Task<IList<CampaignProspect>> GetAllCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(CacheKeys.AllHalIds, out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
                if (campaignProspects.Count > 0)
                {
                    _memoryCache.Set(halId, campaignProspects, TimeSpan.FromMinutes(5));
                }
            }

            return campaignProspects;
        }
    }
}
