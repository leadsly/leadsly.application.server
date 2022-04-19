using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesCommandHandler : ScanProspectsForRepliesCommandHandlerBase, ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            IHalRepository halRepository,
            ILogger<ScanProspectsForRepliesCommand> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            ) : base(logger, campaignRepositoryFacade, rabbitMQProvider, halRepository, timestampService)
        {
            _halRepository = halRepository;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }
        private readonly IHalRepository _halRepository;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<ScanProspectsForRepliesCommand> _logger;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            await InternalExecute(command);
        }

        private async Task InternalExecute(ScanProspectsForRepliesCommand command)
        {
            ScanProspectsForRepliesBody messageBody = await CreateMessageBodyAsync(command);

            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = messageBody.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<ScanProspectsForRepliesBody> CreateMessageBodyAsync(ScanProspectsForRepliesCommand command)
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(command.HalId);

            // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
            string scanprospectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
            ScanProspectsForRepliesBody messageBody = await CreateScanProspectsForRepliesBodyAsync(scanprospectsForRepliesPhaseId, halUnit.HalId, command.UserId);

            return messageBody;
        }
    }
}
