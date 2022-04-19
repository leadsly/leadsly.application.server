using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommand : ICommand
    {
        public MonitorForNewConnectionsCommand(string halId, string userId)
        {
            HalId = halId;
            UserId = userId;
        }

        public string HalId { get; set; }
        public string UserId { get; set; }
    }
}
