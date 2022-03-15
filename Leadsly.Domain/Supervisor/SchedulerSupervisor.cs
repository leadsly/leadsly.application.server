using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task ProcessAllCampaignsAsync()
        {
            // 1. Grab all campaigns that are active
            List<Campaign> activeCampaigns = await _campaignRepository.GetAllActiveAsync();

            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();

            try
            {
                string halId = Environment.GetEnvironmentVariable("HAL_ID", EnvironmentVariableTarget.User);
                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;

                var factory = new ConnectionFactory()
                {
                    UserName = options.ConnectionFactoryOptions.UserName,
                    Password = options.ConnectionFactoryOptions.Password,
                    HostName = options.ConnectionFactoryOptions.HostName,
                    Port = options.ConnectionFactoryOptions.Port,
                    ClientProvidedName = "active.campaigns_factory"
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    foreach (Campaign campaign in activeCampaigns)
                    {
                        foreach (Phase campaignPhase in campaign.Phases)
                        {
                            channel.ExchangeDeclare(exchangeName, exchangeType);

                            channel.QueueDeclare(queue: $"{halId}.follow.up.message",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                            string routingKey = options.RoutingKey.Replace("{halId}", halId);
                            routingKey = routingKey.Replace("{purpose}", "follow-up-messages");
                            channel.QueueBind($"{halId}.follow.up.message", exchangeName, routingKey, null);

                            string message = "Hello World!";
                            var body = Encoding.UTF8.GetBytes(message);
                            // enqueue all phases
                            Publish(channel, exchangeName, routingKey, body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Publish(IModel channel, string exchange, string routingKey, byte[] body)
        {
            channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
        }
    }
}
