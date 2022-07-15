using Leadsly.Domain.Models.Entities.Campaigns;

namespace Leadsly.Domain.Campaigns
{
    public abstract class CampaignType
    {
        public abstract Campaign GeneratePhases(Campaign campaign, bool existingProspectList);
    }
}
