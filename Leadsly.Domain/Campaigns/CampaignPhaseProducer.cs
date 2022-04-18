using Hangfire;
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
        /// <summary>
        /// Triggered once on recurring basis. This phase is triggered once per registered Hal id. This is a passive phase that is supposed to run from the beginning of the work day until the end
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Triggered once a new campaign is created. The intent is to ensure that this phase is running before we execute a new campaign.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task PublishMonitorForNewConnectionsPhaseMessageAsync(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // TODO
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
        /// <summary>
        /// FollowUpMessage triggered by MonitorForNewProspectsPhase. If new prospect accepts our connection invite and the follow up message delay falls during Hal's work day
        /// This method is triggered to send out the message to that specific campaign prospect. The campaign prospect is found by the campaignProspectFollowUpMessageId parameter
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// FollowUpMessagePhase triggered after DeepScanProspectsForRepliesPhase is finished running. This phase is triggered by hal once
        /// DeepScanProspectsForRepliesPhase has completed running, which runs every morning. Runs FollowUpMessagePhase on all eligible campaign prospects for the given 
        /// Hal id that meet the following conditions. CampaignProspect accepted connection request, has not replied and has gotten a follow up message.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task PublishFollowUpMessagesPhaseAsync(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
                foreach (Campaign activeCampaign in campaigns)
                {
                    // grab all campaign prospects for each campaign
                    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                    // filter the list down to only those campaign prospects who have not yet been contacted
                    List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == true).ToList();

                    await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                }
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

        /// <summary>
        /// Triggered every morning and is meant to go out to those prospects who have accepted our invite, who have NOT replied and have NOT gotten a follow up message.
        /// Scenario: On Monday at 6:45 PM, we get a new connection that accepts our request. The follow up message is set up to go out 5 hours after the connection is created.
        /// Hal work day ends at 7:00 PM on Monday. This means that Monday 6:45 PM plus 5 hours = 11:45 PM. The message falls outside of Hal's work day and will not be triggered Monday.
        /// Tuesday morning, this phase is triggered and it checks for all campaign prospects that are part of active campaign that have accepted our invite (our prospect has),
        /// who have NOT received a follow up message (our prospect has not) and who have not replied (naturally our prospect has not replied since they did not have any messages to respond to).
        /// We then check to see if the follow up message delay (5 hours) plus connection accepted date time (Monday 6:45 PM) falls BEFORE Tuesday 7:00AM. If it does we fire the follow up message
        /// right away. If it falls sometime during the work day, we schedule it for that time, if it falls after the work day we do nothing and let the next recurring job take care of it.
        /// </summary>
        /// <returns></returns>
        public async Task PublishUncontactedFollowUpMessagePhaseMessagesAsync()
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
                    List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == false).ToList();

                    await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                }
            }
        }
        #endregion

        #region ProspectListPhase
        /// <summary>
        /// Triggered upon the creation of new Campaign. If campaign created also needs to create new ProspectList, this phase is executed first. This will then create
        /// PrimaryProspectList, PrimaryProspects, and CampaignProspects.
        /// </summary>
        /// <param name="prospectListPhaseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Triggered on recurring bases once in the morning. This phase is meant to trigger to create ProspectLists for campaigns that were created outside of Hal work hours.
        /// This means if user created campaign at 10:00PM local time, the campaign ProspectListPhase will not be triggered until the following morning.
        /// </summary>
        /// <returns></returns>
        public async Task PublishProspectListPhaseMessagesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                HalsProspectListPhasesPayload payload = await campaignProvider.GetIncompleteProspectListPhasesAsync();
                
                // only fire off ProspectListPhase for incomplete phases
                if(payload.ProspectListPayload.Any() == true)
                {
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

        /// <summary>
        /// Triggered by a new campaign that is using existing ProspectList or when ProspectListPhase finishes and we're ready to send out connections for the given campaign.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
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
                PublishSendConnectionsToProspectsPhaseMessages(message, halId);
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

        /// <summary>
        /// Triggered on recurring basis once a day. The purpose of this phase is to perform a deep analysis of the conversation history with any campaign prospect to who meets the following conditions
        /// Has accepted our connection request, has gotten a follow up message and has NOT yet replied to our message. This ensures that we can campture responses from campaign prospects
        /// even if leadsly user has communicated with the prospect themselves. Once Hal is finished with this phase, it will send a request to the application server to first trigger 
        /// FollowUpMessagePhase and then ScanProspectsForRepliesPhase
        /// </summary>
        /// <returns></returns>
        public async Task PublishDeepScanProspectsForRepliesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();

                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsAsync();
                IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();
                foreach (Campaign activeCampaign in campaigns)
                {
                    // grab all campaign prospects for each campaign
                    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                    IList<CampaignProspect> contactedProspects = campaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false).ToList();
                    
                    if(contactedProspects.Count > 0)
                    {
                        string halId = activeCampaign.HalId;
                        halsCampaignProspects.Add(halId, contactedProspects);
                    }                    
                }

                RabbitMQOptions options = GetRabbitMQOptions(rabbitMQRepository);
                ISerializerFacade serializer = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();

                foreach (var halCampaignProspects in halsCampaignProspects)
                {
                    HalUnit halUnit = await halRepository.GetByHalIdAsync(halCampaignProspects.Key);                    
                    string scanProspectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
                    string userId = halUnit.SocialAccount.UserId;

                    // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                    ScanProspectsForRepliesBody messageBody = await rabbitMQProvider.CreateScanProspectsForRepliesBodyAsync(scanProspectsForRepliesPhaseId, halUnit.HalId, userId, halCampaignProspects.Value);
                    ProcessScanProspectsForRepliesPhase(messageBody, serializer, options, RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan);
                }
            }
        }

        /// <summary>
        /// Triggered by Hal when the DeepScanProspectsForRepliesPhase is completed
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task PublishScanProspectsForRepliesPhaseAsync(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();

                RabbitMQOptions options = GetRabbitMQOptions(rabbitMQRepository);
                ISerializerFacade serializer = scope.ServiceProvider.GetRequiredService<ISerializerFacade>();

                HalUnit halUnit = await halRepository.GetByHalIdAsync(halId);

                // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                string scanprospectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
                ScanProspectsForRepliesBody messageBody = await rabbitMQProvider.CreateScanProspectsForRepliesBodyAsync(scanprospectsForRepliesPhaseId, halUnit.HalId, userId);
                ProcessScanProspectsForRepliesPhase(messageBody, serializer, options, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);
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

        private ICommand _command;

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public async Task ExecuteAsync()
        {
            await _command.ExecuteAsync();
        }
    }
}
