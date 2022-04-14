using Leadsly.Application.Model.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface ICampaignPhaseProducer
    {
        Task PublishProspectListPhaseMessagesAsync();
        Task PublishProspectListPhaseMessagesAsync(string prospectListPhaseId, string userId);
        Task PublishConstantCampaignPhaseMessagesAsync();
        Task PublishMonitorForNewConnectionsPhaseMessageAsync();
        void PublishConnectionWithdrawPhaseMessages();
        void PublishSendConnectionsToProspectsPhaseMessages(byte[] body, string halId);
        Task PublishSendConnectionsPhaseMessageAsync(string campaignId, string userId);        

        /// <summary>
        /// Publishes follow up messages to prospects who have accepted our connection invite. This method is triggered by MonitorForNewProspectConnectionsPhase
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        Task PublishFollowUpMessagePhaseMessageAsync(string campaignProspectFollowUpMessageId, string campaignId);

        /// <summary>
        /// Only publishes follow up messages to prospects who have accepted our connection request but have not yet received any follow up messages
        /// </summary>
        /// <returns></returns>
        Task PublishFollowUpMessagePhaseMessagesAsync();

        Task PublishScanProspectsForRepliesFromOffHoursAsync();
    }
}
