using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RestartResourcesCommandHandler : ICommandHandler<RestartResourcesCommand>
    {
        public RestartResourcesCommandHandler(IMessageBrokerOutlet messageBrokerOutlet, ILogger<RestartResourcesCommandHandler> logger)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<RestartResourcesCommandHandler> _logger;

        public Task HandleAsync(RestartResourcesCommand command)
        {
            string queueNameIn = RabbitMQConstants.RestartApplication.QueueName;
            string routingKeyIn = RabbitMQConstants.RestartApplication.RoutingKey;
            string halId = command.HalId;

            RestartApplicationBody messageBody = new();
            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, null);

            return Task.CompletedTask;
        }
    }
}
