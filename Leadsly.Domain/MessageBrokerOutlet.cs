using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.RabbitMQ;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class MessageBrokerOutlet : IMessageBrokerOutlet
    {
        public MessageBrokerOutlet(IRabbitMQManager rabbitMQManager, ISerializerFacade serializerFacade, ILogger<MessageBrokerOutlet> logger)
        {
            _rabbitMQManager = rabbitMQManager;
            _serializerFacade = serializerFacade;
            _logger = logger;
        }

        private readonly IRabbitMQManager _rabbitMQManager;
        private readonly ISerializerFacade _serializerFacade;
        private readonly ILogger<MessageBrokerOutlet> _logger;

        public void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId, IDictionary<string, object> headers)
        {
            byte[] body = _serializerFacade.Serialize(messageBody);

            _logger.LogInformation($"Publishing {messageBody.GetType().Name}.");

            _rabbitMQManager.PublishMessage(body, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
