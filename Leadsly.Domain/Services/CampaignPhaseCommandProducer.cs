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
        public CampaignPhaseCommandProducer(
            ILogger<CampaignPhaseCommandProducer> logger,
            IMessageBrokerOutlet messageBrokerOutlet,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            ICampaignProvider campaignProvider,
            IUserProvider userProvider,
            ITimestampService timestampService,
            ISendFollowUpMessageProvider sendFollowUpMessageProvider
            )
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _campaignProvider = campaignProvider;
            _userProvider = userProvider;
            _timestampService = timestampService;
            _sendFollowUpMessageProvider = sendFollowUpMessageProvider;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;
        private readonly IUserProvider _userProvider;
        private readonly ITimestampService _timestampService;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
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
                ILogger<MonitorForNewConnectionsAllCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<MonitorForNewConnectionsAllCommand>>();

                MonitorForNewConnectionsAllCommand command = new MonitorForNewConnectionsAllCommand(
                       _messageBrokerOutlet,
                       logger,
                        _userProvider,
                        _campaignRepositoryFacade,
                        _halRepository,
                        _timestampService,
                        _rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateProspectListsCommand()
        {
            _logger.LogDebug("Creating ProspectListsCommand for recurring job");

            using (var scope = _serviceProvider.CreateScope())
            {
                ILogger<ProspectListsCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ProspectListsCommand>>();

                ProspectListsCommand command = new ProspectListsCommand(
                    logger,
                    _messageBrokerOutlet,
                    _campaignProvider,
                    _campaignRepositoryFacade,
                    _halRepository,
                    _timestampService,
                    _rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateFollowUpMessagePhaseForUncontactedProspectsCommand()
        {
            _logger.LogDebug("Creating FollowUpMessagePhaseForUncontactedProspectsCommand for recurring job");
            using (var scope = _serviceProvider.CreateScope())
            {
                ILogger<UncontactedFollowUpMessageCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<UncontactedFollowUpMessageCommand>>();

                UncontactedFollowUpMessageCommand command = new UncontactedFollowUpMessageCommand(
                        _messageBrokerOutlet,
                        _campaignRepositoryFacade,
                        logger,
                        _halRepository,
                        _sendFollowUpMessageProvider,
                        _rabbitMQProvider
                    );
                return command;
            }
        }

        private ICommand CreateDeepScanProspectsForRepliesCommand()
        {
            _logger.LogDebug("Creating DeepScanProspectsForRepliesCommand for recurring job");
            using (var scope = _serviceProvider.CreateScope())
            {
                ILogger<DeepScanProspectsForRepliesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<DeepScanProspectsForRepliesCommand>>();

                DeepScanProspectsForRepliesCommand command = new DeepScanProspectsForRepliesCommand(
                    _messageBrokerOutlet,
                    logger,
                    _halRepository,
                    _rabbitMQProvider,
                    _timestampService,
                    _campaignRepositoryFacade
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
                ILogger<ProspectListCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ProspectListCommand>>();

                ProspectListCommand command = new ProspectListCommand(
                        logger,
                        _messageBrokerOutlet,
                        _campaignRepositoryFacade,
                        _halRepository,
                        _timestampService,
                        _rabbitMQProvider,
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
                ILogger<SendConnectionsToProspectsCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<SendConnectionsToProspectsCommand>>();

                SendConnectionsToProspectsCommand command = new SendConnectionsToProspectsCommand(
                        _messageBrokerOutlet,
                        logger,
                        _campaignRepositoryFacade,
                        _halRepository,
                        _timestampService,
                        _rabbitMQProvider,
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
                ILogger<ScanProspectsForRepliesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<ScanProspectsForRepliesCommand>>();

                ScanProspectsForRepliesCommand command = new ScanProspectsForRepliesCommand(
                        _messageBrokerOutlet,
                        _halRepository,
                        logger,
                        _campaignProvider,
                        _campaignRepositoryFacade,
                        _timestampService,
                        _rabbitMQProvider,
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
                ILogger<FollowUpMessagesCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<FollowUpMessagesCommand>>();

                FollowUpMessagesCommand command = new FollowUpMessagesCommand(
                        _messageBrokerOutlet,
                        logger,
                        _halRepository,
                        _rabbitMQProvider,
                        _campaignRepositoryFacade,
                        _sendFollowUpMessageProvider,
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
                ILogger<FollowUpMessageCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<FollowUpMessageCommand>>();

                FollowUpMessageCommand command = new FollowUpMessageCommand(
                        _messageBrokerOutlet,
                        logger,
                        _campaignRepositoryFacade,
                        _halRepository,
                        _rabbitMQProvider,
                        campaignProspectFollowUpMessageId,
                        campaignId, scheduleTime
                    );
                return command;
            }
        }

        #endregion
    }
}
