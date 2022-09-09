using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;

namespace Leadsly.Domain
{
    public interface IMessageBrokerOutlet
    {
        public void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId, IDictionary<string, object> headers);
    }
}
