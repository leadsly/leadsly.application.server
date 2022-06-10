using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.NetworkingHandler
{
    public class NetworkingCommand : ICommand
    {
        public NetworkingCommand(string campaignId, string userId)
        {
            UserId = userId;
            CampaignId = campaignId;
        }

        public NetworkingCommand(IList<string> halIds)
        {
            HalIds = halIds;
        }

        public NetworkingCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
        public IList<string> HalIds { get; set; }
        public string UserId { get; set; }
        public string CampaignId { get; set; }
    }
}
