using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.Campaigns
{
    [DataContract]
    public class CloneCampaignViewModel
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
