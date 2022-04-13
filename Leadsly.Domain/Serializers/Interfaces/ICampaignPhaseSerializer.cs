using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Serializers.Interfaces
{
    public interface ICampaignPhaseSerializer
    {
        byte[] Serialize(ProspectListBody content);

        byte[] Serialize(MonitorForNewAcceptedConnectionsBody content);

        byte[] Serialize(ScanProspectsForRepliesBody content);

        byte[] Serialize(SendConnectionsBody content);

        byte[] Serialize(FollowUpMessageBody content);
    }
}
