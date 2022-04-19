using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface ICampaignManager
    {
        void AddCommand(ICommand command);
        void SetCommand(ICommand command);
        void SetCommands(IList<ICommand> commands);
        Task ExecuteAsync();
        Task ExecuteAllAsync();

        //Task ProcessAllActiveCampaignsAsync();
        //Task TriggerProspectListPhaseAsync(string prospectListPhaseId, string userId);
        //Task TriggerSendConnectionsPhaseAsync(string campaignId, string userId);
        //Task TriggerScanProspectsForRepliesPhaseAsync(string halId, string userId);
        //Task TriggerFollowUpMessagesPhaseAsync(string halId, string userId);
        //Task TriggerMonitorForNewProspectsPhaseAsync(string halId, string userId);        
        //Task TriggerFollowUpMessagePhaseAsync(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime = default);
    }
}
