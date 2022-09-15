using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Microsoft.Extensions.Logging;

namespace Leadsly.Domain.Factories
{
    public class PersistBrowserProfileMessageFactory : IPersistBrowserProfileMessageFactory
    {
        private readonly ILogger<PersistBrowserProfileMessageFactory> _logger;

        public PersistBrowserProfileMessageFactory(
            ILogger<PersistBrowserProfileMessageFactory> logger)
        {
            _logger = logger;
        }

        public PublishMessageBody CreateMQMessage(string halId)
        {
            _logger.LogDebug("Creating {0} MQ message", nameof(PersistBrowserProfileMessageFactory));


            PublishMessageBody mqMessage = new UploadBrowserProfileMessageBody()
            {
                HalId = halId
            };

            return mqMessage;
        }
    }
}
