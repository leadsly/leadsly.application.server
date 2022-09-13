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

        public PublishMessageBody CreateMQMessage()
        {
            _logger.LogDebug("Creating {0} MQ message", nameof(PersistBrowserProfileMessageFactory));

            // currently this is an empty message. All the information required to upload the browser profile to S3 bucket is already in the container set as ENV VARs
            PublishMessageBody mqMessage = new UploadBrowserProfileMessageBody();

            return mqMessage;
        }
    }
}
