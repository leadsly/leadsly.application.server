using Leadsly.Models.Responses.Hal;
using Leadsly.Models.ViewModels.Response.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class WebDriverConverter
    {
        public static INewWebDriverResponseViewModel Convert(INewWebDriverResponse response)
        {
            return new NewWebDriverResponseViewModel
            {
                Failures = FailureConverter.ConvertList(response.Failures),
                Succeeded = response.Succeeded,
                WebDriverId = response.WebDriverId
            };
        }
    }
}
