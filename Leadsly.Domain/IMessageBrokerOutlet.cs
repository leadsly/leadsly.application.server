using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface IMessageBrokerOutlet
    {
        void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId, Dictionary<string, object> headers);
    }
}
