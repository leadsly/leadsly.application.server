using Leadsly.Domain.Campaigns.Commands;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CampaignPhaseCommandProducer : ICampaignPhaseCommandProducer
    {
        public CampaignPhaseCommandProducer(ILogger<CampaignPhaseCommandProducer> logger, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private readonly ILogger<CampaignPhaseCommandProducer> _logger;
        private readonly IServiceProvider _serviceProvider;

        #region RecurringJobs

        public IList<ICommand> CreateRecurringJobCommands()
        {
            IList<ICommand> commands = new List<ICommand>();

            commands.Add(CreateMonitorForNewConnectionsCommand());

            commands.Add(CreateProspectListsCommand());

            commands.Add(CreateFollowUpMessagePhaseForUncontactedProspectsCommand());

            commands.Add(CreateDeepScanProspectsForRepliesCommand());

            return commands;
        }

        private ICommand CreateMonitorForNewConnectionsCommand()
        {
            _logger.LogDebug("Creating MonitorForNewConnectionsCommand for recurring job");

            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<MonitorForNewConnectionsAllCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<MonitorForNewConnectionsAllCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                IUserProvider userProvider = scope.ServiceProvider.GetRequiredService<IUserProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                MonitorForNewConnectionsAllCommand command = new MonitorForNewConnectionsAllCommand(
                        messageBrokerOutlet,
                        logger,
                        userProvider,
                        campaignRepositoryFacade,
                        halRepository,
                        timestampService,
                        rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateProspectListsCommand()
        {
            _logger.LogDebug("Creating ProspectListsCommand for recurring job");

            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<ProspectListsCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ProspectListsCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                ProspectListsCommand command = new ProspectListsCommand(
                    logger,
                    messageBrokerOutlet,
                    campaignProvider,
                    campaignRepositoryFacade,
                    halRepository,
                    timestampService,
                    rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateFollowUpMessagePhaseForUncontactedProspectsCommand()
        {
            _logger.LogDebug("Creating FollowUpMessagePhaseForUncontactedProspectsCommand for recurring job");
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<UncontactedFollowUpMessageCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<UncontactedFollowUpMessageCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ISendFollowUpMessageProvider sendFollowUpMessageProvider = scope.ServiceProvider.GetRequiredService<ISendFollowUpMessageProvider>();

                UncontactedFollowUpMessageCommand command = new UncontactedFollowUpMessageCommand(
                        messageBrokerOutlet,
                        campaignRepositoryFacade,
                        logger,
                        halRepository,
                        sendFollowUpMessageProvider,
                        rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateDeepScanProspectsForRepliesCommand()
        {
            _logger.LogDebug("Creating DeepScanProspectsForRepliesCommand for recurring job");
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<DeepScanProspectsForRepliesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<DeepScanProspectsForRepliesCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                DeepScanProspectsForRepliesCommand command = new DeepScanProspectsForRepliesCommand(
                    messageBrokerOutlet,
                    logger,
                    halRepository,
                    rabbitMQProvider,
                    timestampService,
                    campaignRepositoryFacade
                    );
                return command;
            }
        }

        #endregion

        #region MonitorForNewProspects

        public ICommand CreateMonitorForNewProspectsCommand(string halId, string userId)
        {
            _logger.LogDebug("Creating CreateMonitorForNewProspectsCommand. HalId: {halId}, UserId: {userId}", halId, userId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();                

                MonitorForNewConnectionsCommand command = new MonitorForNewConnectionsCommand(
                        messageBrokerOutlet,                         
                        halId, 
                        userId);
                return command;
            }
        }

        #endregion

        #region ProspectList

        public ICommand CreateProspectListCommand(string prospectListPhaseId, string userId)
        {
            _logger.LogDebug("Creating CreateProspectListCommand. ProspectListPhaseId: {prospectListPhaseId}, UserId: {userId}", prospectListPhaseId, userId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<ProspectListCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ProspectListCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                ProspectListCommand command = new ProspectListCommand(
                        logger,
                        messageBrokerOutlet, 
                        campaignRepositoryFacade,
                        halRepository,
                        timestampService,
                        rabbitMQProvider,
                        prospectListPhaseId, 
                        userId
                    );
                return command;
            }
        }

        #endregion

        #region SendConnections

        public ICommand CreateSendConnectionsCommand(string campaignId, string userId)
        {
            _logger.LogDebug("Creating CreateSendConnectionsCommand. CampaignId: {campaignId}, UserId: {userId}", campaignId, userId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<SendConnectionsToProspectsCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<SendConnectionsToProspectsCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                SendConnectionsToProspectsCommand command = new SendConnectionsToProspectsCommand(
                        messageBrokerOutlet, 
                        logger,
                        campaignRepositoryFacade,
                        halRepository,
                        timestampService,
                        rabbitMQProvider,
                        campaignId, 
                        userId
                    );
                return command;
            }
        }

        #endregion

        #region ScanProspectsForReplies

        public ICommand CreateScanProspectsForRepliesCommand(string halId, string userId)
        {
            _logger.LogDebug("Creating CreateScanProspectsForRepliesCommand. HalId: {halId}, UserId: {userId}", halId, userId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<ScanProspectsForRepliesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ScanProspectsForRepliesCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                ScanProspectsForRepliesCommand command = new ScanProspectsForRepliesCommand(
                        messageBrokerOutlet,
                        halRepository,
                        logger,
                        campaignProvider,
                        campaignRepositoryFacade,
                        timestampService,
                        rabbitMQProvider,
                        halId,
                        userId
                    );
                return command;
            }
        }

        #endregion

        #region FollowUpMessage

        public ICommand CreateFollowUpMessagesCommand(string halId)
        {
            _logger.LogDebug("Creating CreateSendConnectionsCommand. HalId: {halId}, UserId: {userId}", halId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<FollowUpMessagesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<FollowUpMessagesCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();
                ISendFollowUpMessageProvider sendFollowUpMessageProvider = scope.ServiceProvider.GetRequiredService<ISendFollowUpMessageProvider>();

                FollowUpMessagesCommand command = new FollowUpMessagesCommand(
                        messageBrokerOutlet,
                        logger,
                        halRepository,
                        rabbitMQProvider,
                        campaignRepositoryFacade,
                        sendFollowUpMessageProvider,
                        halId
                    );
                return command;
            }
        }

        public ICommand CreateFollowUpMessageCommand(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime)
        {
            _logger.LogDebug("Creating CreateFollowUpMessageCommand. CampaignProspectFollowUpMessageId: {campaignProspectFollowUpMessageId}, CampaignId: {campaignId}", campaignProspectFollowUpMessageId, campaignId);
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<FollowUpMessageCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<FollowUpMessageCommand>>();
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();


                FollowUpMessageCommand command = new FollowUpMessageCommand(
                        messageBrokerOutlet,
                        logger,
                        campaignRepositoryFacade,
                        halRepository,
                        rabbitMQProvider,
                        campaignProspectFollowUpMessageId,
                        campaignId, scheduleTime
                    );
                return command;
            }
        }

        #endregion
    }
}
