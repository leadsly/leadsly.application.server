using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
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
    public class SendConnectionsToProspectsCommand : ICommand
    {
        public SendConnectionsToProspectsCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider, ILogger<SendConnectionsToProspectsCommand> logger, string campaignId, string userId)            
        {
            _campaignId = campaignId;
            _userId = userId;
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private readonly string _campaignId;
        private readonly string _userId;        
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;        
        private readonly ILogger<SendConnectionsToProspectsCommand> _logger;

        /// <summary>
        /// Triggered by a new campaign that is using existing ProspectList or when ProspectListPhase finishes and we're ready to send out connections for the given campaign.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            SendConnectionsBody messageBody = await CreateMessageBodyAsync();
            IList<SendConnectionsStageBody> stages = await CreateStagesAsync(messageBody);
            await SchedulePhaseMessagesAsync(messageBody, stages);
        }

        private async Task SchedulePhaseMessagesAsync(SendConnectionsBody messageBody, IList<SendConnectionsStageBody> stages)
        {
            foreach (SendConnectionsStageBody stage in stages)
            {
                await SchedulePhaseMessageAsync(messageBody, stage);
            }
        }

        private async Task SchedulePhaseMessageAsync(SendConnectionsBody messageBody, SendConnectionsStageBody stage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                string queueNameIn = RabbitMQConstants.NetworkingConnections.QueueName;
                string routingKeyIn = RabbitMQConstants.NetworkingConnections.RoutingKey;
                string halId = messageBody.HalId;

                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.SendConnectionRequests);

                ITimestampService timestampService = scope.ServiceProvider.GetRequiredService<ITimestampService>();

                messageBody.SendConnectionsStage = stage;

                // TODO needs to be adjusted for DateTimeOffset and user's timeZoneId
                DateTimeOffset now = await timestampService.CreateNowDatetimeOffsetAsync(halId);
                if (DateTimeOffset.TryParse(stage.StartTime, out DateTimeOffset phaseStartDateTime))
                {
                    string startTime = stage.StartTime;
                    _logger.LogError("Failed to parse SendConnectionRequests start time. Tried to parse {startTime}", startTime);
                }

                if (now.TimeOfDay > phaseStartDateTime.TimeOfDay)
                {
                    BackgroundJob.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers), phaseStartDateTime);
                }
                else
                {
                    // temporary to schedule jobs right away                
                    _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
                }
            }
        }

        private async Task<IList<SendConnectionsStageBody>> CreateStagesAsync(SendConnectionsBody messageBody)
        {            
            string campaignId = messageBody.CampaignId;
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IList<SendConnectionsStageBody> sendConnectionsStagesBody = await rabbitMQProvider.GetSendConnectionsStagesAsync(campaignId, messageBody.DailyLimit);

                return sendConnectionsStagesBody;
            }             
        }

        private async Task<SendConnectionsBody> CreateMessageBodyAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                SendConnectionsBody messageBody = await rabbitMQProvider.CreateSendConnectionsBodyAsync(_campaignId, _userId);

                return messageBody;
            }
        }
    }
}
