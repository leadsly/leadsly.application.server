﻿using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
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

        #region MonitorForNewConnectionsPhase
        public async Task PublishMonitorForNewConnectionsPhaseMessageAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IUserProvider userProvider = scope.ServiceProvider.GetRequiredService<IUserProvider>();

                IList<SocialAccount> socialAccounts = await userProvider.GetAllSocialAccounts();
                IList<SocialAccount> socialAccountsWithActiveCampaigns = socialAccounts.Where(s => s.User.Campaigns.Any(c => c.Active == true)).ToList();

                RabbitMQOptions options = GetRabbitMQOptions(rabbitMQRepository);

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                // for each hal with active campaigns trigger MonitorForNewProspectsPhase
                foreach (SocialAccount socialAccount in socialAccountsWithActiveCampaigns)
                {
                    MonitorForNewAcceptedConnectionsBody messageBody = await rabbitMQProvider.CreateMonitorForNewAcceptedConnectionsBodyAsync(socialAccount.HalDetails.HalId, socialAccount.UserId, socialAccount.SocialAccountId);

                    ProcessMonitorForNewConnectionsPhase(messageBody, serializerFacade, options);
                }
            }
        }

        private void ProcessMonitorForNewConnectionsPhase(MonitorForNewAcceptedConnectionsBody messageBody, ISerializerFacade serializer, RabbitMQOptions options)
        {
            string halId = messageBody.HalId;
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;
            string userId = messageBody.UserId;

            byte[] body = serializer.Serialize(messageBody);

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            _logger.LogInformation("Publishing MonitorForNewConnectionsPhase. " +
                        "\r\nHal id is: {halId}. " +
                        "\r\nThe queueName is: {queueName} " +
                        "\r\nThe routingKey is: {routingKey} " +
                        "\r\nThe exchangeName is: {exchangeName} " +
                        "\r\nThe exchangeType is: {exchangeType} " +
                        "\r\nUser id is: {userId}",
                        halId,
                        queueName,
                        routingKey,
                        exchangeName,
                        exchangeType,
                        userId
                        );

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }
        #endregion

        #region FollowUpMessagesPhase
        public async Task PublishFollowUpMessagePhaseMessageAsync(string campaignProspectFollowUpMessageId, string campaignId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();

                // grab the CampaignProspectFollowUpMessage
                FollowUpMessageBody followUpMessageBody = await rabbitMQProvider.CreateFollowUpMessageBodyAsync(campaignProspectFollowUpMessageId, campaignId);
                RabbitMQOptions options = GetRabbitMQOptions(rabbitMQRepository);
                ISerializerFacade serializer = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();

                ProcessFollowUpMessagePhase(followUpMessageBody, serializer, options);
            }
        }

        private void ProcessFollowUpMessagePhase(FollowUpMessageBody messageBody, ISerializerFacade serializer, RabbitMQOptions options)
        {
            string halId = messageBody.HalId;
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;
            string userId = messageBody.UserId;

            byte[] body = serializer.Serialize(messageBody);

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.FollowUpMessage.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", RabbitMQConstants.FollowUpMessage.QueueName);
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.FollowUpMessage.RoutingKey);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            _logger.LogInformation("Publishing FollowUpMessagePhase. " +
                        "\r\nHal id is: {halId}. " +
                        "\r\nThe queueName is: {queueName} " +
                        "\r\nThe routingKey is: {routingKey} " +
                        "\r\nThe exchangeName is: {exchangeName} " +
                        "\r\nThe exchangeType is: {exchangeType} " +
                        "\r\nUser id is: {userId}",
                        halId,
                        queueName,
                        routingKey,
                        exchangeName,
                        exchangeType,
                        userId
                        );

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        public async Task PublishFollowUpMessagePhaseMessagesAsync()
        {
            // first grab all active campaigns
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsAsync();
                foreach (Campaign activeCampaign in campaigns)
                {
                    // grab all campaign prospects for each campaign
                    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                    // filter the list down to only those campaign prospects who have not yet been contacted
                    List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == false).ToList();

                    await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                }
            }
        }
        #endregion

        #region ProspectListPhase
        public async Task PublishProspectListPhaseMessagesAsync(string prospectListPhaseId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();

                ProspectListBody messageBody = await rabbitMQProvider.CreateProspectListBodyAsync(prospectListPhaseId, userId);

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();

                ProcessProspectListPhase(messageBody, serializerFacade, options, messageBody.HalId, exchangeName, exchangeType, queueName, routingKey, userId);
            }
        }

        private void ProcessProspectListPhase(ProspectListBody messageBody, ISerializerFacade serializer, RabbitMQOptions options, string halId, string exchangeName, string exchangeType, string queueName, string routingKey, string userId)
        {
            byte[] body = serializer.Serialize(messageBody);

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

            _logger.LogInformation("Publishing ProspectListPhase. " +
                "\r\nHal id is: {halId}. " +
                "\r\nThe queueName is: {queueName} " +
                "\r\nThe routingKey is: {routingKey} " +
                "\r\nThe exchangeName is: {exchangeName} " +
                "\r\nThe exchangeType is: {exchangeType} " +
                "\r\nUser id is: {userId}",
                halId,
                queueName,
                routingKey,
                exchangeName,
                exchangeType,
                userId
                );

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

                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.MessageId = Guid.NewGuid().ToString();

                List<ProspectListBody> prospectListBodies = payload.ProspectListPayload[halId];

                foreach (ProspectListBody prospectListBody in prospectListBodies)
                {
                    string userId = prospectListBody.UserId;
                    byte[] body = serializerFacade.Serialize(prospectListBody);

                    _logger.LogInformation("Publishing ProspectListPhase. " +
                        "\r\nHal id is: {halId}. " +
                        "\r\nThe queueName is: {queueName} " +
                        "\r\nThe routingKey is: {routingKey} " +
                        "\r\nThe exchangeName is: {exchangeName} " +
                        "\r\nThe exchangeType is: {exchangeType} " +
                        "\r\nUser id is: {userId}",
                        halId,
                        queueName,
                        routingKey,
                        exchangeName,
                        exchangeType,
                        userId
                        );

                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
                }
            }
        }
        #endregion

        #region SendConnectionRequests
        public async Task PublishSendConnectionsPhaseMessageAsync(string campaignId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();

                SendConnectionsBody messageBody = await rabbitMQProvider.CreateSendConnectionsBodyAsync(campaignId, userId);
                IList<SendConnectionsStageBody> sendConnectionsStagesBody = await rabbitMQProvider.GetSendConnectionsStagesAsync(campaignId, messageBody.DailyLimit);

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                foreach (SendConnectionsStageBody sendConnectionsStageBody in sendConnectionsStagesBody)
                {
                    messageBody.SendConnectionsStage = sendConnectionsStageBody;

                    byte[] message = serializerFacade.Serialize(messageBody);
                    ScheduleSendConnectionsToProspectsPhaseMessages(message, sendConnectionsStageBody.StartTime, messageBody.HalId);
                }
            }
        }

        private void ScheduleSendConnectionsToProspectsPhaseMessages(byte[] message, string phaseStartTime, string halId)
        {
            _logger.LogDebug("Scheduling send connections to prospects phase.");
            // TODO needs to be adjusted for DateTimeOffset and user's timeZoneId
            DateTime now = DateTime.Now;
            DateTime phaseStartDateTime = DateTime.Parse(phaseStartTime);
            if (now.TimeOfDay > phaseStartDateTime.TimeOfDay)
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
            if (_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
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

            _logger.LogInformation("Publishing SendConnectionsToProspectsPhase. " +
                        "\r\nHal id is: {halId}. " +
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

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }
        #endregion

        #region ScanForProspectRepliesPhase

        public async Task PublishScanProspectsForRepliesFromOffHoursAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                //ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();
                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsAsync();
                IList<string> halIds = await campaignProvider.GetHalIdsWithActiveCampaignsAsync();

                //DateTime now = DateTime.Now;
                //DateTime last24Hours = DateTime.Now.AddHours(-24);
                //foreach (Campaign activeCampaign in campaigns)
                //{
                //    // grab all campaign prospects for each campaign
                //    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                //    // filter the list down to only those campaign prospects who have been contacted and received a message in the last 24 hours
                //    IList<CampaignProspect> contactedProspects = new List<CampaignProspect>();                    
                //    foreach (CampaignProspect campaignProspect in campaignProspects)
                //    {
                //        if(campaignProspect.Accepted = true && campaignProspect.FollowUpMessageSent == true) 
                //        {
                //            DateTimeOffset lastMessageSent = DateTimeOffset.FromUnixTimeSeconds(campaignProspect.LastFollowUpMessageSentTimestamp);
                //            DateTimeOffset nowLocal = await timestampService.CreateDatetimeOffsetAsync(campaignProspect.Campaign.HalId, now);
                //            DateTimeOffset last24HoursLocal = await timestampService.CreateDatetimeOffsetAsync(campaignProspect.Campaign.HalId, last24Hours);
                //            // was last message sent within last 24 hours
                //            if (lastMessageSent > last24HoursLocal && lastMessageSent <= nowLocal)
                //            {
                //                contactedProspects.Add(campaignProspect);
                //            }
                //        }                        
                //    }
                //}

                RabbitMQOptions options = GetRabbitMQOptions(rabbitMQRepository);
                ISerializerFacade serializer = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();

                foreach (string halId in halIds)
                {
                    HalUnit halUnit = await halRepository.GetByHalIdAsync(halId);

                    string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
                    string userId = halUnit.SocialAccount.UserId;

                    // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                    ScanProspectsForRepliesBody messageBody = await rabbitMQProvider.CreateScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halId, userId);
                    ProcessScanProspectsForRepliesPhase(messageBody, serializer, options, RabbitMQConstants.ScanProspectsForReplies.ExecuteOnce);
                }
            }
        }

        private void ProcessScanProspectsForRepliesPhase(ScanProspectsForRepliesBody messageBody, ISerializerFacade serializer, RabbitMQOptions options, string executionType)
        {
            string halId = messageBody.HalId;
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;
            string userId = messageBody.UserId;

            byte[] body = serializer.Serialize(messageBody);

            ConnectionFactory factory = ConfigureConnectionFactor(options, RabbitMQConstants.ScanProspectsForReplies.QueueName);
            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", RabbitMQConstants.ScanProspectsForReplies.QueueName);
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.ScanProspectsForReplies.RoutingKey);

            channel.QueueBind(queueName, exchangeName, routingKey, null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();            
            basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, executionType);

            _logger.LogInformation("Publishing ScanProspectsForRepliesPhase. " +
                        "\r\nHal id is: {halId}. " +
                        "\r\nThe queueName is: {queueName} " +
                        "\r\nThe routingKey is: {routingKey} " +
                        "\r\nThe exchangeName is: {exchangeName} " +
                        "\r\nThe exchangeType is: {exchangeType} " +
                        "\r\nUser id is: {userId}",
                        halId,
                        queueName,
                        routingKey,
                        exchangeName,
                        exchangeType,
                        userId
                        );

            channel.BasicPublish(exchange: options.ExchangeOptions.Name, routingKey: routingKey, basicProperties: basicProperties, body: body);
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

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.MessageId = Guid.NewGuid().ToString();

            ScanProspectsForRepliesBody content = new()
            {
                HalId = halId
            };

            byte[] body = serializerFacade.Serialize(content);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: basicProperties, body: body);
        }

        public async Task PublishConstantCampaignPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                // get all halIds that have active campaigns
                List<string> halIdsWithActiveCampaigns = await campaignProvider.GetHalIdsWithActiveCampaignsAsync();
                halIdsWithActiveCampaigns = new List<string> { Environment.GetEnvironmentVariable("HAL_ID", EnvironmentVariableTarget.User) };

                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();

                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;
                string queueName = options.QueueConfigOptions.Name;
                string routingKey = options.RoutingKey;

                ISerializerFacade serializerFacade = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();
                ProcessConstantCampaignPhase(options, halIdsWithActiveCampaigns, exchangeName, exchangeType, queueName, routingKey, serializerFacade);
            }
        }

        private void ProcessConstantCampaignPhase(RabbitMQOptions options, List<string> halIds, string exchangeName, string exchangeType, string queueName, string routingKey, ISerializerFacade serializerFacade)
        {
            foreach (string halId in halIds)
            {
                PublishScanProspectsForRepliesPhaseMessages(options, halId, exchangeName, exchangeType, queueName, routingKey, serializerFacade);
            }
        }
        #endregion

        #region ConnectionWithdrawnPhase
        public void PublishConnectionWithdrawPhaseMessages()
        {
            throw new NotImplementedException();
        }
        #endregion
        private RabbitMQOptions GetRabbitMQOptions(IRabbitMQRepository rabbitMQRepository)
        {
            RabbitMQOptions options = default;
            if (_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
            {
                options = rabbitMQRepository.GetRabbitMQConfigOptions();

                _memoryCache.Set(CacheKeys.RabbitMQConfigOptions, options, DateTimeOffset.Now.AddMinutes(10));
            }
            return options;
        }                                
    }
}
