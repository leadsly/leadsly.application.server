using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.ViewModels.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Expired = campaign.Expired
            };
        }
    }
}
