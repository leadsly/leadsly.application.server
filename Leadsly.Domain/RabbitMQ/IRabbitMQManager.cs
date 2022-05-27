using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface IRabbitMQManager
    {
        public void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn, string halId, Dictionary<string, object> headers = default);
    }
}
