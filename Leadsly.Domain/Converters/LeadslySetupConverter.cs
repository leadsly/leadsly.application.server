using Leadsly.Domain.ViewModels;
using Leadsly.Domain.ViewModels.Cloud;
using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class LeadslySetupConverter
    {
        public static LeadslySetupResultViewModel Convert(LeadslySetupResultDTO dto)
        {
            return new LeadslySetupResultViewModel
            {
               RequiresNewCloudResource = dto.RequiresNewCloudResource,
               Succeeded = dto.Succeeded,
               Failures = FailureConverter.ConvertList(dto.Failures)               
            };
        }
    }
}
