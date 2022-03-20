using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Serializers.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Serializers
{
    public class CampaignPhaseSerializer : ICampaignPhaseSerializer
    {
        public byte[] SerializeMonitorForNewAcceptedConnections(MonitorForNewAcceptedConnectionsBody content)
        {
            string message = JsonConvert.SerializeObject(content);
            return Encoding.UTF8.GetBytes(message);
        }

        public byte[] SerializeProspectList(ProspectListBody content)
        {
            string message = JsonConvert.SerializeObject(content);
            return Encoding.UTF8.GetBytes(message);
        }

        public byte[] SerializeScanProspectsForReplies(ScanProspectsForRepliesBody content)
        {
            string message = JsonConvert.SerializeObject(content);
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
