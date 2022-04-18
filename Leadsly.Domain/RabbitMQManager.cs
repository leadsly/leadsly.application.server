using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RabbitMQManager : IRabbitMQManager
    {
        public RabbitMQManager(ILogger<RabbitMQManager> logger, IRabbitMQRepository rabbitMQRepository, IMemoryCache memoryCache)
        {
            _logger = logger;
            _rabbitMQRepository = rabbitMQRepository;
            _memoryCache = memoryCache;
        }

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<RabbitMQManager> _logger;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly List<IConnection> Connections = new();
        private readonly List<IModel> Channels = new();

        private ConnectionFactory ConfigureConnectionFactory(RabbitMQOptions options, string clientProviderName)
        {
            return new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryOptions.UserName,
                Password = options.ConnectionFactoryOptions.Password,
                HostName = options.ConnectionFactoryOptions.HostName,
                Port = options.ConnectionFactoryOptions.Port,
                ClientProvidedName = clientProviderName,
                DispatchConsumersAsync = true
            };
        }

        public void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn, string halId, Dictionary<string, object> headers = default)
        {
            RabbitMQOptions options = GetRabbitMQOptions();
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;

            ConnectionFactory factory = ConfigureConnectionFactory(options, queueNameIn);

            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", queueNameIn);
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", routingKeyIn);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();
            if(headers != default)
            {
                basicProperties.Headers = headers;
            }

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);
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
