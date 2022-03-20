using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICampaignPhaseProducerFacade
    {
        void PublishProspectListMessagesPhase(List<string> phaseIds);
        void PublishConstantCampaignMessagesPhase();
        void PublishSendConnectionsToProspectsMessagesPhase();
        void PublishConnectionWithdrawMessagesPhase();
    }
}
