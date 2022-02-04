using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class ECSTask
    {
        public string Id { get; set; }
        public string ECSServiceId { get; set; }
        public string UserId { get; set; }
        public ECSService ECSService { get; set; }
    }
}
