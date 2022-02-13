using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICloudPlatformRepository
    {
        CloudPlatformConfiguration GetCloudPlatformConfiguration();
    }
}
