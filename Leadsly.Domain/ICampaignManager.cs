using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface ICampaignManager
    {
        Task ProcessAllActiveCampaignsAsync();        

        void TriggerProspectListPhase(string prospectListPhaseId, string userId);

        void TriggerSendConnectionsPhase(string campaignId, string userId);

        Task TriggerFollowUpMessagePhaseAsync(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime = default);
    }
}
