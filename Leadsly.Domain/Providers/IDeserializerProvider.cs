using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface IDeserializerProvider
    {
        Task<HalOperationResult<T>> DeserializeConnectAccountResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;
    }
}
