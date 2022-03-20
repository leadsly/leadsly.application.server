using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Deserializers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades
{
    public class DeserializerFacade : IDeserializerFacade
    {
        public DeserializerFacade(ILogger<DeserializerFacade> logger, IConnectAccountResponseDeserializer connectAccountResponseDeserializer, IEnterTwoFactorAuthCodeResponseDeserializer enterTwoFactorAuthCodeResponseDeserializer)
        {
            _logger = logger;
            _connectAccountResponseDeserializer = connectAccountResponseDeserializer;
            _enterTwoFactorAuthCodeResponseDeserializer = enterTwoFactorAuthCodeResponseDeserializer;
        }

        private readonly ILogger<DeserializerFacade> _logger;
        private readonly IConnectAccountResponseDeserializer _connectAccountResponseDeserializer;
        private readonly IEnterTwoFactorAuthCodeResponseDeserializer _enterTwoFactorAuthCodeResponseDeserializer;

        public async Task<HalOperationResult<T>> DeserializeConnectAccountResponseAsync<T>(HttpResponseMessage response) where T : IOperationResponse
        {
            return await _connectAccountResponseDeserializer.DeserializeAsync<T>(response);
        }

        public async Task<HalOperationResult<T>> DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(HttpResponseMessage response) where T : IOperationResponse
        {
            return await _enterTwoFactorAuthCodeResponseDeserializer.DeserializeAsync<T>(response);
        }
    }
}
