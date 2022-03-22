using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class CampaignPhaseProducer : ICampaignPhaseProducer
    {
        public CampaignPhaseProducer(ILogger<CampaignPhaseProducer> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        ~CampaignPhaseProducer()
        {
            _logger.LogInformation("Destructing campaign phase producer");
            foreach (IModel channel in Channels)
            {
                try
                {
                    channel.Close();
                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error occured when closing rabbit mq channels");
                }
            }

            foreach (IConnection connection in Connections)
            {
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error occured when closing rabbit mq connections");
                }
            }
        }

        private readonly List<IConnection> Connections = new();
        private readonly List<IModel> Channels = new();

        private readonly ILogger<CampaignPhaseProducer> _logger;
        private readonly IServiceProvider _serviceProvider;

        private ConnectionFactory ConfigureConnectionFactor(RabbitMQOptions options, string clientProviderName)
        {
            return new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryOptions.UserName,
                Password = options.ConnectionFactoryOptions.Password,
                HostName = options.ConnectionFactoryOptions.HostName,
                Port = options.ConnectionFactoryOptions.Port,
                ClientProvidedName = clientProviderName
            };
        }

        public async Task PublishProspectListPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignPhaseProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignPhaseProvider>();
                HalsProspectListPhasesPayload payload = await campaignProvider.GetActiveProspectListPhasesAsync();

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ConnectionFactory factory = ConfigureConnectionFactor(options, "prospect.lists");
                var connection = factory.CreateConnection();
                Connections.Add(connection);
                var channel = connection.CreateModel();
                Channels.Add(channel);

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                ProcessProspectListPhases(channel, payload, exchangeName, exchangeType, queueName, routingKey, serializerFacade);
            }
        }

        private void ProcessProspectListPhases(IModel channel, HalsProspectListPhasesPayload payload, string exchangeName, string exchangeType, string queueName, string routingKey, ISerializerFacade serializerFacade)
        {
            channel.ExchangeDeclare(exchangeName, exchangeType);

            foreach (string halId in payload.ProspectListPayload.Keys)
            {
                string name = queueName.Replace("{halId}", halId);
                channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

                routingKey = routingKey.Replace("{halId}", halId);
                routingKey = routingKey.Replace("{purpose}", "prospect-list");

                channel.QueueBind(name, exchangeName, routingKey, null);

                channel.BasicAcks += (sender, ea) =>
                {
                    // emitted when consumer does basic acknowledge

                    // from here we have to trigger send connections phase
                    PublishSendConnectionsToProspectsPhaseMessages();
                };
                channel.BasicNacks += (sender, ea) =>
                {
                    // emitted when consumer does basic negative acknowledge
                };

                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.MessageId = Guid.NewGuid().ToString();

                List<ProspectListBody> prospectListBodies = payload.ProspectListPayload[halId];

                foreach (ProspectListBody prospectListBody in prospectListBodies)
                {
                    byte[] body = serializerFacade.SerializeProspectList(prospectListBody);
                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
                }
            }
        }

        public async Task PublishConstantCampaignPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignPhaseProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignPhaseProvider>();
                // get all halIds that have active campaigns
                List<string> halIdsWithActiveCampaigns = await campaignProvider.HalIdsWithActiveCampaignsAsync();
                halIdsWithActiveCampaigns = new List<string> { Environment.GetEnvironmentVariable("HAL_ID", EnvironmentVariableTarget.User) };

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                ProcessConstantCampaignPhase(options, halIdsWithActiveCampaigns,  exchangeName, exchangeType, queueName, routingKey, serializerFacade);
            }
        }

        private void ProcessConstantCampaignPhase(RabbitMQOptions options, List<string> halIds, string exchangeName, string exchangeType, string queueName, string routingKey, ISerializerFacade serializerFacade)
        {            
            foreach (string halId in halIds)
            {
                PublishScanProspectsForRepliesPhaseMessages(options, halId, exchangeName, exchangeType, queueName, routingKey, serializerFacade);

                PublishMonitorNewAcceptedConnectionsPhaseMessages(options, halId, exchangeName, exchangeType, queueName, routingKey, serializerFacade);
            }
        }

        private void PublishScanProspectsForRepliesPhaseMessages(RabbitMQOptions options, string halId, string exchangeName, string exchangeType, string queueName, string routingKey, ISerializerFacade serializerFacade)
        {
            ConnectionFactory factory = ConfigureConnectionFactor(options, "scan.prospects.for.replies");
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = queueName.Replace("{halId}", halId);
            channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            routingKey = routingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", "scan-prospects-for-replies");

            channel.QueueBind(name, exchangeName, routingKey, null);

            channel.BasicAcks += (sender, ea) =>
            {
                // emitted when consumer does basic acknowledge

            };
            channel.BasicNacks += (sender, ea) =>
            {
                // emitted when consumer does basic negative acknowledge
            };

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            ScanProspectsForRepliesBody content = new()
            {
                HalId = halId
            };

            byte[] body = serializerFacade.SerializeScanProspectsForReplies(content);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        private void PublishMonitorNewAcceptedConnectionsPhaseMessages(RabbitMQOptions options, string halId, string exchangeName, string exchangeType, string queueName, string routingKey, ISerializerFacade serializerFacade)
        {
            ConnectionFactory factory = ConfigureConnectionFactor(options, "monitor.new.connections");
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = queueName.Replace("{halId}", halId);
            name = name.Replace("{queueName}", RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            routingKey = routingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey);

            channel.QueueBind(name, exchangeName, routingKey, null);

            channel.BasicAcks += (sender, ea) =>
            {
                // emitted when consumer does basic acknowledge

            };
            channel.BasicNacks += (sender, ea) =>
            {
                // emitted when consumer does basic negative acknowledge
            };

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            MonitorForNewAcceptedConnectionsBody content = new()
            {
                HalId = halId
            };

            byte[] body = serializerFacade.SerializeMonitorForNewAcceptedConnections(content);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        private void PublishSendConnectionsToProspectsPhaseMessages()
        {
            // check if warm up options is checked

            // figure out number of connections we can send out for this campaign a day

            // determine the cadance at which the connections will be sent out (between 7:00am - 10:00pm) this would be every hour, three or custom perhaps?

            // check where send connections left off at

            // get a starting point, and ending point of the connections to send and the page the connection is on


            throw new NotImplementedException();
        }

        public void PublishConnectionWithdrawPhaseMessages()
        {
            throw new NotImplementedException();
        }
    }
}
