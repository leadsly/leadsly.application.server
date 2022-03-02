using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class LeadslySetupConverter
    {
        public static SetupAccountResultViewModel Convert(LeadslySetupResultDTO dto)
        {
            return new SetupAccountResultViewModel
            {
               RequiresNewCloudResource = dto.RequiresNewCloudResource,
               Succeeded = dto.Succeeded,
               Failures = FailureConverter.ConvertList(dto.Failures)               
            };
        }
               
    }
}
