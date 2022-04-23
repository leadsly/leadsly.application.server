using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.SendConnectionsToProspectsHandlers
{
    public class SendConnectionsToProspectsCommand : ICommand
    {
        public SendConnectionsToProspectsCommand(string userId, string campaignId)
        {
            UserId = userId;
            CampaignId = campaignId;
        }

        public SendConnectionsToProspectsCommand(IList<string> halIds)
        {
            HalIds = halIds;
        }

        public IList<string> HalIds { get; set; }
        public string UserId { get; set; }
        public string CampaignId { get; set; }
    }
}
