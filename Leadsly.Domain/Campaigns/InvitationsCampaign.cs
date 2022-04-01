using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns
{
    public class InvitationsCampaign : CampaignType
    {
        public override Campaign GeneratePhases(Campaign campaign, bool existingProspectList)
        {
            SendConnectionRequestPhase sendConnectionsPhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.SendConnectionRequests
            };
            campaign.SendConnectionRequestPhase = sendConnectionsPhase;

            ConnectionWithdrawPhase connectionWithdrawPhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.ConnectionWithdraw
            };
            campaign.ConnectionWithdrawPhase = connectionWithdrawPhase;

            if(existingProspectList == false)
            {
                ProspectListPhase prospectListPhase = new()
                {
                    Campaign = campaign,
                    Completed = false,
                    PhaseType = PhaseType.ProspectList,
                    SearchUrls = campaign.CampaignProspectList.SearchUrls.Select(url => url.Url).ToList()
                };
                campaign.ProspectListPhase = prospectListPhase;
            }
            return campaign;
        }
    }
}
