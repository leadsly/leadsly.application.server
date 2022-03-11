using Leadsly.Application.Model.Entities.Campaigns;
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

            try
            {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                foreach (Campaign campaign in activeCampaigns)
                {
                    foreach (Phase campaignPhase in campaign.Phases)
                    {
                        channel.QueueDeclare(queue: "prospect-list-queue-hal123",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                        string message = "Hello World!";
                        var body = Encoding.UTF8.GetBytes(message);
                        // enqueue all phases
                        Publish("prospect-list-queue-hal123", channel, "", "hal123-routing-key", body);
                    }
                }
            }
            }
            catch (Exception ex)
            {

            }
        }

        private void Publish(string queueName, IModel channel, string exchange, string routingKey, byte[] body)
        {
            channel.BasicPublish(exchange: exchange, routingKey: "prospect-list-queue-hal123", basicProperties: null, body: body);
        }
    }
}
