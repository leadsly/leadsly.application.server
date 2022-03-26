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
        Task PublishProspectListPhaseMessagesAsync(string prospectListPhaseId);
        Task PublishConstantCampaignPhaseMessagesAsync();
        void PublishConnectionWithdrawPhaseMessages();

        Task PublishSendConnectionsPhaseMessageAsync(string campaignId);
    }
}
