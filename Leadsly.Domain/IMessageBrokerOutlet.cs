using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;

namespace Leadsly.Domain
{
    public interface IMessageBrokerOutlet
    {
        [ExecuteOncePhasesOnce]
        public void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string hangfireUniqueId, string routingKeyIn, string halId, IDictionary<string, object> headers);
    }
}
