﻿using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace Leadsly.Domain.RabbitMQ
{
    public interface IRabbitMQManager
    {
        public void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn, string halId, IDictionary<string, object> headers = default);

        public void StartConsuming(string queueNameIn, string routingKeyIn, AsyncEventHandler<BasicDeliverEventArgs> receivedHandlerAsync);
    }
}
