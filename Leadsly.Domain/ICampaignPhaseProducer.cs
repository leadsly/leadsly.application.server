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
    }
}
