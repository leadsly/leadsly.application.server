using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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

namespace Leadsly.Domain.Providers
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(ILogger<CampaignProvider> logger, ICampaignService campaignService, ICampaignManager campaignManager, ICampaignRepository campaignRepository)
        {
            _campaignService = campaignService;
            _logger = logger;
            _campaignManager = campaignManager;
            _campaignRepository = campaignRepository;
        }

        private readonly ILogger<CampaignProvider> _logger;
        private readonly ICampaignManager _campaignManager;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICampaignService _campaignService;
        public void ProcessNewCampaign(Campaign campaign)
        {
            // if prospect list phase does not exists, this means were running new prospect list
            if(campaign.ProspectListPhase == null)
            {                
                _campaignManager.TriggerSendConnectionsPhase(campaign.Id);
            }
            else
            {
                _campaignManager.TriggerProspectListPhase(campaign.ProspectListPhase.Id);
            }
        }

        public async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            ProspectListPhase prospectListPhase = await _campaignRepository.GetProspectListPhaseByIdAsync(prospectListPhaseId, ct);
            ProspectListBody prospectListBody = new()
            {
                SearchUrls = prospectListPhase.SearchUrls,
                HalId = prospectListPhase.Campaign.HalId
            };

            return prospectListBody;
        }

        public async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, CancellationToken ct = default)
        {
            Campaign campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId, ct);
            int dailyConnectionCount = await _campaignService.SetDailyLimitAsync(campaign, ct);
            SentConnectionsStatus sentConnectionsStatus = await _campaignRepository.GetSentConnectionStatusAsync(campaignId, ct);
            IEnumerable<CampaignProspect> campaignProspects = await _campaignRepository.GetCampaignProspectsByIdAsync(campaignId, ct);
            IEnumerable<CampaignProspectBody> notConnectedProspects = campaignProspects.Where(c => c.ConnectionSent == false).Select(c =>
            {
                return new CampaignProspectBody()
                {
                    Name = c.Name,
                    ProfileUrl = c.ProfileUrl
                };
            });

            SendConnectionsBody sendConnectionsBody = new()
            {
                ChromeProfileName = "",
                DailyLimit = dailyConnectionCount,
                HalId = campaign.HalId,
                PageUrl = sentConnectionsStatus.LastVisistedPageUrl.Url,
                LastProspectHitListPosition = sentConnectionsStatus.LastProspectHitListPosition,
                NextPageUrl = sentConnectionsStatus.NextPageUrl.Url,
                StartDateTimestamp = campaign.StartTimestamp,
                Prospects = notConnectedProspects
            };

            return sendConnectionsBody;
        }

        public CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            CampaignProspectList campaignProspectList = _campaignService.GenerateCampaignProspectList(primaryProspectList, userId);

            return campaignProspectList;
        }

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
