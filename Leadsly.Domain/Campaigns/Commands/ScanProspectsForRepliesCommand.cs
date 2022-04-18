using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class ScanProspectsForRepliesCommand : ICommand
    {
        public ScanProspectsForRepliesCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider, string halId, string userId)
        {
            _halId = halId;
            _userId = userId;
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly string _halId;
        private readonly string _userId;
        private readonly IServiceProvider _serviceProvider;
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
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IHalRepository halRepository = scope.ServiceProvider.GetRequiredService<IHalRepository>();

                HalUnit halUnit = await halRepository.GetByHalIdAsync(_halId);

                // fire off ScanProspectsForRepliesPhase with the payload of the contacted prospects
                string scanprospectsForRepliesPhaseId = halUnit.SocialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
                ScanProspectsForRepliesBody messageBody = await rabbitMQProvider.CreateScanProspectsForRepliesBodyAsync(scanprospectsForRepliesPhaseId, halUnit.HalId, _userId);

                return messageBody;
            }
        }
    }
}
