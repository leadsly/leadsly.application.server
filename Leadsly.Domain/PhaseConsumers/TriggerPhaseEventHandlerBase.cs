using Leadsly.Domain.Models.RabbitMQMessages;

namespace Leadsly.Domain.PhaseConsumers
{
    public abstract class TriggerPhaseEventHandlerBase
    {
        protected abstract TriggerPhaseMessageBodyBase DeserializeMessage(string rawMessage);
    }
}
