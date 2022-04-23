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
            if(command.HalIds != null)
            {
                await InternalHandleListAsync(command.HalIds);
            }

            if (command.HalId != null)
            {
                await InternalHandleAsync(command.HalId);
            }
        }

        private async Task InternalHandleListAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                await InternalHandleAsync(halId);
            }
        }

        private async Task InternalHandleAsync(string halId)
        {
            ScanProspectsForRepliesBody messageBody = await CreateMessageBodyAsync(halId);

            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<ScanProspectsForRepliesBody> CreateMessageBodyAsync(string halId)
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);

            // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
            string scanprospectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
            ScanProspectsForRepliesBody messageBody = await CreateScanProspectsForRepliesBodyAsync(scanprospectsForRepliesPhaseId, halId);

            return messageBody;
        }
    }
}
