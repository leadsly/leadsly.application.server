using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class HalOperationConverter        
    {
        public static HalOperationResultViewModel<T> Convert<T>(HalOperationResult<IConnectAccountResponse> result)
            where T : IOperationResponseViewModel
        {
            return new HalOperationResultViewModel<T>
            {
                OperationResults = new ResultBaseViewModel
                {
                    Failures = FailureConverter.ConvertList(result.Failures),
                    ProblemDetails = result.ProblemDetails,
                    Succeeded = result.Succeeded,
                    WebDriverError = result.WebDriverError,
                    WindowHandleId = result.Value?.OperationInformation?.WindowHandleId,
                    TabClosed = result.Value?.OperationInformation?.TabClosed ?? false,
                    BrowserClosed = result.Value?.OperationInformation?.BrowserClosed ?? false,
                    ShouldOperationBeRetried = result.ShouldOperationBeRetried
                }                
            };
        }

        public static HalOperationResultViewModel<T> Convert<T>(HalOperationResult<IEnterTwoFactorAuthCodeResponse> result)
            where T : IOperationResponseViewModel
        {
            return new HalOperationResultViewModel<T>
            {
                OperationResults = new ResultBaseViewModel
                {
                    Failures = FailureConverter.ConvertList(result.Failures),
                    ProblemDetails = result.ProblemDetails,
                    Succeeded = result.Succeeded,
                    WebDriverError = result.WebDriverError,
                    WindowHandleId = result.Value?.OperationInformation?.WindowHandleId,
                    TabClosed = result.Value?.OperationInformation?.TabClosed ?? false,
                    ShouldOperationBeRetried = result.ShouldOperationBeRetried
                }
            };
        }

        public static HalOperationResultViewModel<T> Convert<T>(HalOperationResult<INewWebDriverResponse> result)
            where T : IOperationResponseViewModel
        {
            return new HalOperationResultViewModel<T>
            {
                OperationResults = new ResultBaseViewModel
                {
                    Failures = FailureConverter.ConvertList(result.Failures),
                    ProblemDetails = result.ProblemDetails,
                    Succeeded = result.Succeeded,
                    WebDriverError = result.WebDriverError,
                    WindowHandleId = result.Value?.OperationInformation?.WindowHandleId,
                    TabClosed = result.Value?.OperationInformation?.TabClosed ?? false,
                    ShouldOperationBeRetried = result.ShouldOperationBeRetried
                }
            };
        }
    }
}
