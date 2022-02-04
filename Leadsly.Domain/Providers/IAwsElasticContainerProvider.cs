using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface IAwsElasticContainerProvider
    {
        Task<CreateContainerResultDTO> SetupUsersContainer();
    }
}
