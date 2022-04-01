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
    public class FollowUpCampaign : CampaignType
    {

        public override Campaign GeneratePhases(Campaign campaign, bool existingProspectList)
        {
            if(existingProspectList == false)
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

            SendConnectionRequestPhase sendConnectionRequestPhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.SendConnectionRequests
            };
            campaign.SendConnectionRequestPhase = sendConnectionRequestPhase;

            ScanProspectsForRepliesPhase scanProspectsForRepliesPhase = new()
            {
                Campaign = campaign,                
                PhaseType = PhaseType.ScanForReplies
            };
            campaign.ScanProspectsForRepliesPhase = scanProspectsForRepliesPhase;

            FollowUpMessagesPhase followUpMessagePhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.FollwUpMessages
            };
            campaign.FollowUpMessagePhase = followUpMessagePhase;

            ConnectionWithdrawPhase connectionWithdrawPhase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.ConnectionWithdraw
            };
            campaign.ConnectionWithdrawPhase = connectionWithdrawPhase;

            return campaign;
        }
    }
}
