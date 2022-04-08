using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns
{
    public class CampaignPhaseProducer : ICampaignPhaseProducer
    {
        public CampaignPhaseProducer(ILogger<CampaignPhaseProducer> logger, IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
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
        private readonly IMemoryCache _memoryCache;

        private ConnectionFactory ConfigureConnectionFactor(RabbitMQOptions options, string clientProviderName)
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

        public async Task PublishMonitorForNewConnectionsPhaseMessageAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                
                List<Campaign> activeCampaigns = await campaignProvider.GetActiveCampaignsAsync();                

                RabbitMQOptions options = default;
                if (_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
                {
                    IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                    options = rabbitMQRepository.GetRabbitMQConfigOptions();

                    _memoryCache.Set(CacheKeys.RabbitMQConfigOptions, options);
                }

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                // for each hal with active campaigns trigger MonitorForNewProspectsPhase
                foreach (Campaign activeCampaign in activeCampaigns)
                {
                    MonitorForNewAcceptedConnectionsBody messageBody = await campaignProvider.CreateMonitorForNewAcceptedConnectionsBodyAsync(activeCampaign.HalId, activeCampaign.ApplicationUserId);

                    ProcessMonitorForNewConnectionsPhase(messageBody, serializerFacade, options);
                }
            }
        }

        private void ProcessMonitorForNewConnectionsPhase(MonitorForNewAcceptedConnectionsBody messageBody, ISerializerFacade serializer, RabbitMQOptions options)
        {
            byte[] body = serializer.SerializeMonitorForNewAcceptedConnections(messageBody);

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(options.ExchangeOptions.Name, options.ExchangeOptions.ExchangeType);

            string name = options.QueueConfigOptions.Name.Replace("{halId}", messageBody.HalId);
            name = name.Replace("{queueName}", RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", messageBody.HalId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey);

            channel.QueueBind(name, options.ExchangeOptions.Name, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        public async Task PublishProspectListPhaseMessagesAsync(string prospectListPhaseId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();

                ProspectListBody messageBody = await campaignProvider.CreateProspectListBodyAsync(prospectListPhaseId, userId);

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();                

                ProcessProspectListPhases(messageBody, serializerFacade, options, messageBody.HalId, exchangeName, exchangeType, queueName, routingKey, userId);
            }
        }

        private void ProcessProspectListPhases(ProspectListBody messageBody, ISerializerFacade serializer, RabbitMQOptions options, string halId, string exchangeName, string exchangeType, string queueName, string routingKey, string userId)
        {
            byte[] body = serializer.SerializeProspectList(messageBody);

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.NetworkingConnections.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = queueName.Replace("{halId}", halId);
            name = name.Replace("{queueName}", RabbitMQConstants.NetworkingConnections.QueueName);
            channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            routingKey = routingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.NetworkingConnections.RoutingKey);

            channel.QueueBind(name, exchangeName, routingKey, null);
                        
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();
            basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.ProspectList);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        public async Task PublishProspectListPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                HalsProspectListPhasesPayload payload = await campaignProvider.GetActiveProspectListPhasesAsync();

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.NetworkingConnections.QueueName);
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
                routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.NetworkingConnections.RoutingKey);

                channel.QueueBind(name, exchangeName, routingKey, null);

                channel.BasicAcks += (sender, ea) =>
                {
                    // emitted when consumer does basic acknowledge

                    // from here we have to trigger send connections phase
                    // PublishSendConnectionsToProspectsPhaseMessages();
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

        public async Task PublishSendConnectionsPhaseMessageAsync(string campaignId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();

                SendConnectionsBody messageBody = await campaignProvider.CreateSendConnectionsBodyAsync(campaignId, userId);
                IList<SendConnectionsStageBody> sendConnectionsStagesBody = await campaignProvider.GetSendConnectionsStagesAsync(campaignId, messageBody.DailyLimit);

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                foreach (SendConnectionsStageBody sendConnectionsStageBody in sendConnectionsStagesBody)
                {
                    messageBody.SendConnectionsStage = sendConnectionsStageBody;

                    byte[] message = serializerFacade.SerializeSendConnections(messageBody);
                    ScheduleSendConnectionsToProspectsPhaseMessages(message, sendConnectionsStageBody.StartTime, messageBody.HalId);
                }
            }
        }

        private void ScheduleSendConnectionsToProspectsPhaseMessages(byte[] message, string phaseStartTime, string halId)
        {
            DateTime now = DateTime.Now;
            DateTime phaseStartDateTime = DateTime.Parse(phaseStartTime);
            if(now.TimeOfDay > phaseStartDateTime.TimeOfDay)
            {
                BackgroundJob.Schedule<ICampaignPhaseProducer>(x => x.PublishSendConnectionsToProspectsPhaseMessages(message, halId), phaseStartDateTime);                
            }
            else
            {
                // temporary to schedule jobs right away
                BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishSendConnectionsToProspectsPhaseMessages(message, halId));
            }
            
        }

        public void PublishSendConnectionsToProspectsPhaseMessages(byte[] body, string halId)
        {
            RabbitMQOptions options = default;
            if(_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                    options = rabbitMQRepository.GetRabbitMQConfigOptions();

                    _memoryCache.Set(CacheKeys.RabbitMQConfigOptions, options);
                }
            }

            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;
            string queueName = options.QueueConfigOptions.Name;
            string routingKey = options.RoutingKey;

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.NetworkingConnections.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = queueName.Replace("{halId}", halId);
            name = name.Replace("{queueName}", RabbitMQConstants.NetworkingConnections.QueueName);
            channel.QueueDeclare(queue: name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            routingKey = routingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.NetworkingConnections.RoutingKey);

            channel.QueueBind(name, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();
            basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.SendConnectionRequests);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        public async Task PublishConstantCampaignPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
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
        
        public void PublishConnectionWithdrawPhaseMessages()
        {
            throw new NotImplementedException();
        }
    }
}
