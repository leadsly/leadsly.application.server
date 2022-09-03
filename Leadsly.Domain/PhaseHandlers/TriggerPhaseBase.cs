using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.RabbitMQ;
using System;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers
{
    public abstract class TriggerPhaseBase
    {
        protected virtual Task PublishMessageAsync(PublishMessageBody message)
        {
            throw new NotImplementedException();
        }

        protected virtual Task TriggerPhaseAsync(TriggerFollowUpMessageBody message)
        {
            throw new NotImplementedException();
        }
    }
}
