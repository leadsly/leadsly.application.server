using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Models.ViewModels.Campaigns;

namespace Leadsly.Domain.Converters
{
    public static class CampaignConverter
    {
        public static CampaignViewModel Convert(Campaign campaign)
        {
            return new CampaignViewModel
            {
                Id = campaign.CampaignId,
                Active = campaign.Active,
                Name = campaign.Name,
                ConnectionsSentDaily = campaign.DailyInvites,
                Expired = campaign.Expired,
                ConnectionsAccepted = 0,
                Notes = string.Empty,
                ProfileViews = 0,
                Replies = 0,
                TotalConnectionsSent = 0
            };
        }

    }
}
