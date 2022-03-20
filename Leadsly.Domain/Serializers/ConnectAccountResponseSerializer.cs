﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Domain.Serializers.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Serializers
{
    public class ConnectAccountResponseSerializer : IConnectAccountResponseSerializer
    {
        public ConnectAccountResponseSerializer(ILogger<ConnectAccountResponseSerializer> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<ConnectAccountResponseSerializer> _logger;

        public async Task<HalOperationResult<T>> DeserializeAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            try
            {
                _logger.LogInformation("Attempting to deserialize response.");
                string content = await response.Content.ReadAsStringAsync();
                IConnectAccountResponse resp = JsonConvert.DeserializeObject<ConnectAccountResponse>(content);
                response.Headers.TryGetValues(CustomHeaderKeys.Origin, out IEnumerable<string> customOriginHeaders);
                resp.OperationInformation.HalId = customOriginHeaders?.FirstOrDefault();
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = (T)resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from hal");
                result.Failures.Add(new()
                {
                    Code = Codes.DESERIALIZATION_ERROR,
                    Reason = "Failed to deserialize response",
                    Detail = "Failed to deserialize response content into an object"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}