using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class MessageBrokerOutlet : IMessageBrokerOutlet
    {
        public MessageBrokerOutlet(IRabbitMQManager rabbitMQManager, ISerializerFacade serializerFacade)
        {
            _rabbitMQManager = rabbitMQManager;
            _serializerFacade = serializerFacade;
        }

        private readonly IRabbitMQManager _rabbitMQManager;
        private readonly ISerializerFacade _serializerFacade;

        public void PublishPhase(PublishMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId, Dictionary<string, object> headers)
        {
            byte[] body = _serializerFacade.Serialize(messageBody);

            //_logger.LogInformation("Publishing MonitorForNewConnectionsPhase. " +
            //            "\r\nHal id is: {halId}. " +
            //            "\r\nThe queueName is: {queueName} " +
            //            "\r\nThe routingKey is: {routingKey} " +
            //            "\r\nThe exchangeName is: {exchangeName} " +
            //            "\r\nThe exchangeType is: {exchangeType} " +
            //            "\r\nUser id is: {userId}",
            //            halId,
            //            queueName,
            //            routingKey,
            //            exchangeName,
            //            exchangeType,
            //            userId
            //            );

            _rabbitMQManager.PublishMessage(body, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
