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
        byte[] SerializeProspectList(ProspectListBody content);

        byte[] SerializeMonitorForNewAcceptedConnections(MonitorForNewAcceptedConnectionsBody content);

        byte[] SerializeScanProspectsForReplies(ScanProspectsForRepliesBody content);
    }
}
