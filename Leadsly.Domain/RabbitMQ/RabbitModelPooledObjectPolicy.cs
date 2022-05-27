﻿using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.RabbitMQ
{
    public class RabbitModelPooledObjectPolicy : PooledObjectPolicy<IModel>
    {
        public RabbitModelPooledObjectPolicy(RabbitMQConfigOptions options)
        {
            _connection = GetConnection(options);
        }

        private readonly IConnection _connection;

        private IConnection GetConnection(RabbitMQConfigOptions options)
        {
            var factory = new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryConfigOptions.UserName,
                Password = options.ConnectionFactoryConfigOptions.Password,
                HostName = options.ConnectionFactoryConfigOptions.HostName,
                Port = options.ConnectionFactoryConfigOptions.Port,                                
                DispatchConsumersAsync = true,
                ClientProvidedName = "[Publisher] AppServer"
            };

            return factory.CreateConnection();
        }

        public override IModel Create()
        {
            return _connection.CreateModel();
        }

        public override bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }
}
