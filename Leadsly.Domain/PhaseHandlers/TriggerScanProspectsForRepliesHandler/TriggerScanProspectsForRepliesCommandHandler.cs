using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.TriggerScanProspectsForRepliesHandler
{
    public class TriggerScanProspectsForRepliesCommandHandler : TriggerPhaseBase, ICommandHandler<TriggerScanProspectsForRepliesCommand>
    {
        public TriggerScanProspectsForRepliesCommandHandler(
            ILogger<TriggerScanProspectsForRepliesCommandHandler> logger,
            ICreateScanProspectsForRepliesMessageProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        private readonly ICreateScanProspectsForRepliesMessageProvider _provider;
        private readonly ILogger<TriggerScanProspectsForRepliesCommandHandler> _logger;

        public async Task HandleAsync(TriggerScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            await TriggerPhaseAsync(command.Message);

            channel.BasicAck(args.DeliveryTag, false);
        }

        protected override async Task TriggerPhaseAsync(TriggerPhaseMessageBodyBase message)
        {
            TriggerScanProspectsForRepliesMessageBody messageBody = message as TriggerScanProspectsForRepliesMessageBody;
            _logger.LogInformation("[TriggerFollowUpMessagesAsync] Exuecting TriggerScanProspectsForRepliesCommand for halId: {0}", messageBody.HalId);
            PublishMessageBody mqMessage = await _provider.CreateMQScanProspectsForRepliesMessageAsync(messageBody.UserId, messageBody.HalId);

            await PublishMessageAsync(mqMessage);
        }
    }
}
