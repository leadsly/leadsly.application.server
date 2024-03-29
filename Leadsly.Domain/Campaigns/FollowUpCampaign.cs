﻿using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Linq;

namespace Leadsly.Domain.Campaigns
{
    public class FollowUpCampaign : CampaignType
    {

        public override Campaign GeneratePhases(Campaign campaign, bool existingProspectList)
        {
            if (existingProspectList == false)
            {
                ProspectListPhase prospectListPhase = new ProspectListPhase
                {
                    Campaign = campaign,
                    Completed = false,
                    PhaseType = PhaseType.ProspectList,
                    SearchUrls = campaign.CampaignProspectList.SearchUrls.Select(url => url.Url).ToList()
                };
                campaign.ProspectListPhase = prospectListPhase;
            }

            FollowUpMessagePhase followUpMessagePhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.FollowUpMessage
            };
            campaign.FollowUpMessagePhase = followUpMessagePhase;

            return campaign;
        }
    }
}
