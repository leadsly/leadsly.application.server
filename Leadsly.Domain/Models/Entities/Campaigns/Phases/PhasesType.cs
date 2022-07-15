using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.Entities.Campaigns.Phases
{
    public enum PhaseType
    {
        None,
        ProspectList,
        MonitorNewConnections,
        Networking,
        ScanForReplies,
        DeepScan,
        FollwUpMessage,
        SendConnectionRequests,
        SendEmailInvites,
        ConnectionWithdraw,
        RescrapeSearchUrls
    }
}
