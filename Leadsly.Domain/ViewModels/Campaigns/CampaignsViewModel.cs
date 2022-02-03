using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.Campaigns
{
    [DataContract]
    public class CampaignsViewModel
    {
        [DataMember(Name = "items")]
        public List<CampaignViewModel> Items { get; set; }
    }
}
