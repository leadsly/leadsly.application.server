﻿using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leadsly.Infrastructure.Repositories
{
    public class RabbitMQRepository : IRabbitMQRepository
    {
        public RabbitMQRepository(IOptions<RabbitMQConfigOptions> rabbitMQConfigOptions, ILogger<RabbitMQRepository> logger)
        {
            _logger = logger;
            _rabbitMQConfigOptions = rabbitMQConfigOptions.Value;
        }

        private readonly ILogger<RabbitMQRepository> _logger;
        private readonly RabbitMQConfigOptions _rabbitMQConfigOptions;

        public RabbitMQOptions GetRabbitMQConfigOptions()
        {
            return new RabbitMQOptions
            {
                RoutingKey = new()
                {
                    AppServer = _rabbitMQConfigOptions.RoutingKey.AppServer,
                    Hal = _rabbitMQConfigOptions.RoutingKey.Hal
                },
                ConnectionFactoryOptions = new()
                {
                    ClientProvidedName = new()
                    {
                        AppServer = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.ClientProvidedName.AppServer,
                        Hal = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.ClientProvidedName.Hal
                    },
                    HostName = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.HostName,
                    Password = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.Password,
                    Port = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.Port,
                    UserName = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.UserName,
                },
                ExchangeOptions = new()
                {
                    AppServer = new()
                    {
                        Name = _rabbitMQConfigOptions.ExchangeConfigOptions.AppServer.Name,
                        ExchangeType = _rabbitMQConfigOptions.ExchangeConfigOptions.AppServer.ExchangeType
                    },
                    Hal = new()
                    {
                        Name = _rabbitMQConfigOptions.ExchangeConfigOptions.Hal.Name,
                        ExchangeType = _rabbitMQConfigOptions.ExchangeConfigOptions.Hal.ExchangeType
                    },
                },
                QueueConfigOptions = new()
                {
                    AppServer = new()
                    {
                        Name = _rabbitMQConfigOptions.QueueConfigOptions.AppServer.Name,
                        AutoAcknowledge = _rabbitMQConfigOptions.QueueConfigOptions.AppServer.AutoAcknowledge,
                    },
                    Hal = new()
                    {
                        Name = _rabbitMQConfigOptions.QueueConfigOptions.Hal.Name,
                        AutoAcknowledge = _rabbitMQConfigOptions.QueueConfigOptions.Hal.AutoAcknowledge,
                    }
                }
            };
        }
    }
}
