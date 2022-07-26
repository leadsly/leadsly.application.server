using Leadsly.Domain.OptionsJsonModels;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

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
                ClientProvidedName = "[Publisher] AppServer",
                Ssl = new SslOption()
                {
                    Enabled = options.ConnectionFactoryConfigOptions.Ssl.Enabled
                }
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
