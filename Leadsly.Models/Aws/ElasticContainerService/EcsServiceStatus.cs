using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class EcsServiceStatus
    {
        public const string ACTIVE = "ACTIVE";
        public const string DRAINING = "DRAINING";
        public const string INACTIVE = "INACTIVE";        
    }
}
