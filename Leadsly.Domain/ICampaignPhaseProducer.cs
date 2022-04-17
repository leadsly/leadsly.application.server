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
        Task PublishScanProspectsForRepliesPhaseAsync(string halId, string userId);
        Task PublishFollowUpMessagesPhaseAsync(string halId, string userId);

        /// <summary>
        /// Publishes follow up messages to prospects who have accepted our connection invite. This method is triggered by MonitorForNewProspectConnectionsPhase.
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        Task PublishFollowUpMessagePhaseMessageAsync(string campaignProspectFollowUpMessageId, string campaignId);

        /// <summary>
        /// Only publishes follow up messages to prospects who have accepted our connection request but have not yet received any follow up messages
        /// </summary>
        /// <returns></returns>
        Task PublishUncontactedFollowUpMessagePhaseMessagesAsync();

        /// <summary>
        /// Triggered only once a day. This will grab all of the campaign prospects who have accepted our invite, have received a follow up message
        /// and have not replied yet. This will grab the last automated follow up message sent to the prospect, and check to see if any
        /// of the messages that are seen after this last message are from the prospect. This is called DeepScan because it scans 
        /// all of the messages between our last sent message and the most recent message. (In case leadsly user decides to have a conversation
        /// with the prospect. We don't want to send follow up messages when that happens)
        /// </summary>
        /// <returns></returns>
        Task PublishDeepScanProspectsForRepliesAsync();
    }
}
