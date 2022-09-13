using Leadsly.Domain.MQ.Messages;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IPersistBrowserProfileMessageFactory
    {
        PublishMessageBody CreateMQMessage();
    }
}
