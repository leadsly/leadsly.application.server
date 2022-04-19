//using Leadsly.Domain.Campaigns.Handlers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Leadsly.Domain.Services.Interfaces
//{
//    public interface ICampaignPhaseCommandProducer
//    {
//        IList<ICommand> CreateRecurringJobCommands();

//        ICommand CreateMonitorForNewProspectsCommand(string halId, string userId);

//        ICommand CreateProspectListCommand(string prospectListPhaseId, string userId);

//        ICommand CreateSendConnectionsCommand(string campaignId, string userId);

//        ICommand CreateScanProspectsForRepliesCommand(string halId, string userId);

//        ICommand CreateFollowUpMessagesCommand(string halId);

//        ICommand CreateFollowUpMessageCommand(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime);
//    }
//}
