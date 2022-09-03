using Leadsly.Domain.PhaseConsumers;
using Leadsly.Domain.PhaseConsumers.TriggerFollowUpMessagesHandler;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ConsumingService : IConsumingService
    {
        public ConsumingService(ILogger<ConsumingService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private readonly ILogger<ConsumingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public async Task StartConsumingAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume TriggerSendFollowUpMessages messages
                ////////////////////////////////////////////////////////////////////////////////////
                IConsumeCommandHandler<TriggerFollowUpMessagesConsumeCommand> triggerFollowUpMessagesHandler = scope.ServiceProvider.GetRequiredService<IConsumeCommandHandler<TriggerFollowUpMessagesConsumeCommand>>();
                TriggerFollowUpMessagesConsumeCommand triggerFollowUpMessagesCommand = new TriggerFollowUpMessagesConsumeCommand();
                await triggerFollowUpMessagesHandler.ConsumeAsync(triggerFollowUpMessagesCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume TriggerScanProspectsForReplies messages
                ////////////////////////////////////////////////////////////////////////////////////
            }
        }
    }
}
