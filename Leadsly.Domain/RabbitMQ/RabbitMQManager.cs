using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Leadsly.Domain
{
    public class RabbitMQManager : IRabbitMQManager
    {
        public RabbitMQManager(ILogger<RabbitMQManager> logger, IRabbitMQRepository rabbitMQRepository, IMemoryCache memoryCache, ObjectPool<IModel> pool)
        {
            _logger = logger;
            _pool = pool;
            _rabbitMQRepository = rabbitMQRepository;
            _memoryCache = memoryCache;
        }

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<RabbitMQManager> _logger;
        private readonly ObjectPool<IModel> _pool;
        private readonly IRabbitMQRepository _rabbitMQRepository;


        public void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn, string halId, Dictionary<string, object> headers = default)
        {
            RabbitMQOptions options = GetRabbitMQOptions();

            string exchangeName = options.ExchangeOptions.Name;            
            string exchangeType = options.ExchangeOptions.ExchangeType;
            
            IModel channel = _pool.Get();

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", queueNameIn);
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", routingKeyIn);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();            
            basicProperties.Expiration = TimeSpan.FromHours(3).TotalMilliseconds.ToString();
            if (headers != default)
            {
                basicProperties.Headers = headers;
            }

            _logger.LogInformation("Hal id is: {halId}. " +
                        "\r\nThe queueName is: {queueName} " +
                        "\r\nThe routingKey is: {routingKey} " +
                        "\r\nThe exchangeName is: {exchangeName} " +
                        "\r\nThe exchangeType is: {exchangeType} ",                        
                        halId,
                        queueName,
                        routingKey,
                        exchangeName,
                        exchangeType
                        );

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);

            _pool.Return(channel);
        }

        private RabbitMQOptions GetRabbitMQOptions()
        {
            RabbitMQOptions options = default;
            if (_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
            {
                options = _rabbitMQRepository.GetRabbitMQConfigOptions();

                _memoryCache.Set(CacheKeys.RabbitMQConfigOptions, options, DateTimeOffset.Now.AddMinutes(10));
            }
            return options;
        }
    }
}
