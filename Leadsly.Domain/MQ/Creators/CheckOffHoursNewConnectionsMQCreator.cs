using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class CheckOffHoursNewConnectionsMQCreator : MQCreatorBase, ICheckOffHoursNewConnectionsMQCreator
    {
        public CheckOffHoursNewConnectionsMQCreator(
            ILogger<CheckOffHoursNewConnectionsMQCreator> logger,
            IUserRepository userRepository,
            ICheckOffHoursNewConnectionsCreateMQService service,
            IMessageBrokerOutlet messageBrokerOutlet)
        {
            _service = service;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
            _userRepository = userRepository;
        }

        private readonly ICheckOffHoursNewConnectionsCreateMQService _service;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<CheckOffHoursNewConnectionsMQCreator> _logger;
        private readonly IUserRepository _userRepository;

        public async Task<PublishMessageBody> CreateMQMessage(string halId, CancellationToken ct = default)
        {
            _logger.LogDebug("Creating MonitorForNewAcceptedConnectionsBody message.");
            SocialAccount socialAccount = await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
            if (socialAccount == null)
            {
                _logger.LogWarning("Unable to locate SocialAccount associated with halId {halId}. Cannot proceed", halId);
                return null;
            }

            string userId = socialAccount.UserId;
            MonitorForNewConnectionsPhase phase = socialAccount.MonitorForNewProspectsPhase;
            PublishMessageBody mqMessage = await _service.CreateMQMessageAsync(userId, halId, phase, ct);

            return mqMessage;
        }

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            PublishMessageBody mqMessage = await CreateMQMessage(halId, ct);

            if (mqMessage != null)
            {
                _logger.LogInformation("Publishing ScanProspectsForReplies MQ message for HalId {halId}", halId);
                PublishMessage(mqMessage);
            }
            else
            {
                _logger.LogDebug("ScanProspectsForReplies MQ message was null. Nothing will be published for HalId {halId}", halId);
            }
        }

        protected override void PublishMessage(PublishMessageBody message)
        {
            string halId = message.HalId;
            _logger.LogInformation("[HandleAsync] Exuecting {0} for halId: {1}", nameof(CheckOffHoursNewConnectionsBody), halId);
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            _logger.LogInformation($"Setting rabbitMQ headers. Header key {RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType} header value is {RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan}");
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan);

            _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
