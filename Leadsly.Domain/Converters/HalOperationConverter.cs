using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class HalOperationConverter<U>
        where U : IOperationResponse
    {
        public static HalOperationResultViewModel<T> Convert<T>(HalOperationResult<U> result)
            where T : IOperationResponseViewModel
        {
            return new HalOperationResultViewModel<T>
            {
                Failures = FailureConverter.ConvertList(result.Failures),
                ProblemDetails = result.ProblemDetails,
                Succeeded = result.Succeeded
            };
        }
    }
}
