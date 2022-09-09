using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities;
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
    public class ScanProspectsForRepliesMQCreator : MQCreatorBase, IScanProspectsForRepliesMQCreator
    {
        public ScanProspectsForRepliesMQCreator(
            ILogger<ScanProspectsForRepliesMQCreator> logger,
            IMessageBrokerOutlet messageBrokerOutlet,
            IScanProspectsForRepliesCreateMQService service,
            IUserRepository userRepository
            )
        {
            _service = service;
            _userRepository = userRepository;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IUserRepository _userRepository;
        private readonly IScanProspectsForRepliesCreateMQService _service;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<ScanProspectsForRepliesMQCreator> _logger;

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("[ScanProspectsForRepliesBody] Exuecting {0} for halId: {1}.", halId, nameof(ScanProspectsForRepliesBody));

            SocialAccount socialAccount = await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
            if (socialAccount == null)
            {
                _logger.LogWarning("Unable to locate SocialAccount associated with halId {halId}. Cannot proceed", halId);
                return;
            }

            string userId = socialAccount.UserId;
            string scanProspectsForRepliesPhaseId = socialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
            PublishMessageBody mqMessage = await _service.CreateMQMessageAsync(userId, halId, scanProspectsForRepliesPhaseId, ct);

            if (mqMessage != null)
            {
                _logger.LogInformation($"Publishing {nameof(ScanProspectsForRepliesBody)} MQ message for HalId {halId}", halId);
                PublishMessage(mqMessage);
            }
            else
            {
                _logger.LogDebug($"{nameof(ScanProspectsForRepliesBody)} MQ message was null. Nothing will be published for HalId {halId}", halId);
            }
        }

        protected override void PublishMessage(PublishMessageBody message)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = message.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);

            _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
