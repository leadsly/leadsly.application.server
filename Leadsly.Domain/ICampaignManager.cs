using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface ICampaignManager
    {
        void ProcessAllActiveCampaigns();

        void TriggerProspectListPhase(string prospectListPhaseId, string userId);

        void TriggerSendConnectionsPhase(string campaignId, string userId);
    }
}
