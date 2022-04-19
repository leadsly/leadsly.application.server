using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.RabbitMQ;
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

namespace Leadsly.Domain.Campaigns.Commands
{
    public class ScanProspectsForRepliesCommand : ScanProspectsForRepliesBaseCommand, ICommand
    {
        public ScanProspectsForRepliesCommand(
            IMessageBrokerOutlet messageBrokerOutlet, 
            IHalRepository halRepository,  
            ILogger<ScanProspectsForRepliesCommand> logger,
            ICampaignProvider campaignProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,            
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider,
            string halId, 
            string userId)
            : base(logger, campaignRepositoryFacade, rabbitMQProvider, halRepository, timestampService)
        {
            _halId = halId;
            _userId = userId;
            _campaignProvider = campaignProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _timestampService = timestampService;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ILogger<ScanProspectsForRepliesCommand> _logger;
        private readonly string _halId;
        private readonly string _userId;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ITimestampService _timestampService;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered by Hal when the DeepScanProspectsForRepliesPhase is completed
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {            
            await InternalExecute();
        }

        private async Task InternalExecute()
        {
            ScanProspectsForRepliesBody messageBody = await CreateMessageBodyAsync();

            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = messageBody.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }
        
        private async Task<ScanProspectsForRepliesBody> CreateMessageBodyAsync() 
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(_halId);

            // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
            string scanprospectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
            ScanProspectsForRepliesBody messageBody = await CreateScanProspectsForRepliesBodyAsync(scanprospectsForRepliesPhaseId, halUnit.HalId, _userId);

            return messageBody;
        }        
    }
}
