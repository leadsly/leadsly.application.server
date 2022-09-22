using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Leadsly.Domain.MQ.EventHandlers
{
    public abstract class PhaseEventHandlerBase
    {
        private readonly ILogger _logger;

        public PhaseEventHandlerBase(ILogger logger)
        {
            _logger = logger;
        }
        protected virtual T DeserializeMessage<T>(string rawMessage)
            where T : class
        {
            _logger.LogInformation("Deserializing {0}", typeof(T).Name);
            T message = null;
            try
            {
                message = JsonConvert.DeserializeObject<T>(rawMessage);
                _logger.LogDebug("Successfully deserialized {0}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize {0}. Returning an explicit null", typeof(T).Name);
                return null;
            }

            return message;
        }
    }
}
