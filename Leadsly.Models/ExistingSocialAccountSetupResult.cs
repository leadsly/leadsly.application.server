using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ExistingSocialAccountSetupResult
    {
        public bool EcsServiceActive { get; set; }
        public bool EcsTaskRunning { get; set; }
        public bool EcsServiceHasPendingTasks { get; set; }
        public bool IsHalHealthy { get; set; }
        public string HalName { get; set; }
        public bool Succeeded { get; set; }
        public SocialAccountCloudResourceDTO Value { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
