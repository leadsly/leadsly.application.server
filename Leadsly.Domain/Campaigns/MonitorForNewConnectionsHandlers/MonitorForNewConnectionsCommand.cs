using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommand : ICommand
    {
        public MonitorForNewConnectionsCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
