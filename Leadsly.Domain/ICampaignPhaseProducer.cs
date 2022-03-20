using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface ICampaignPhaseProducer
    {
        Task PublishProspectListPhaseMessagesAsync();
        Task PublishConstantCampaignPhaseMessagesAsync();
        void PublishConnectionWithdrawPhaseMessages();
    }
}
