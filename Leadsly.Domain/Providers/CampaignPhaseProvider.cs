using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignPhaseProvider : ICampaignPhaseProvider
    { 
        public CampaignPhaseProvider(ILogger<CampaignPhaseProducer> logger, ICampaignRepository campaignRepository)
        {
            _campaignRepository = campaignRepository;
            _logger = logger;
        }

        private readonly ICampaignRepository _campaignRepository;
        private readonly ILogger<CampaignPhaseProducer> _logger;

        public async Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            List<ProspectListPhase> prospectListPhases = await _campaignRepository.GetAllActivePropspectListPhasesAsync(ct);

            IEnumerable<string> halIds = prospectListPhases.Select(phase => phase.Campaign.HalId).Distinct();

            HalsProspectListPhasesPayload halsPhases = new();
            foreach (string halId in halIds)
            {
                List<ProspectListBody> content = prospectListPhases.Where(p => p.Campaign.HalId == halId).Select(p => 
                {
                    return new ProspectListBody
                    {
                        SearchUrls = p.SearchUrls
                    };
                }).ToList();

                halsPhases.ProspectListPayload.Add(halId, content);
            }

            return halsPhases;
        }

        public async Task<List<string>> HalIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        {
            List<Campaign> activeCampaigns = await _campaignRepository.GetAllActiveAsync(ct);

            List<string> halIds = activeCampaigns.Select(c => c.HalId).ToList();

            return halIds;
        }
    }
}
