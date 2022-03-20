using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Serializers.Interfaces
{
    public interface IEnterTwoFactorAuthCodeResponseSerializer
    {
        Task<HalOperationResult<T>> DeserializeAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;
    }
}   
