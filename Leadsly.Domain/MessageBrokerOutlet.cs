using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.MQ.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Leadsly.Domain
{
    public class MessageBrokerOutlet : IMessageBrokerOutlet
    {
        public MessageBrokerOutlet(IRabbitMQManager rabbitMQManager, ILogger<MessageBrokerOutlet> logger)
        {
            _rabbitMQManager = rabbitMQManager;
            _logger = logger;
        }

        private readonly IRabbitMQManager _rabbitMQManager;
        private readonly ILogger<MessageBrokerOutlet> _logger;

        public void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId, IDictionary<string, object> headers)
        {
            string message = JsonConvert.SerializeObject(messageBody);
            byte[] rawMessage = Encoding.UTF8.GetBytes(message);

            _logger.LogInformation($"Publishing {messageBody.GetType().Name}.");

            _rabbitMQManager.PublishMessage(rawMessage, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
