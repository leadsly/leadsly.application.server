using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Linq;

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

            if (existingProspectList == false)
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
