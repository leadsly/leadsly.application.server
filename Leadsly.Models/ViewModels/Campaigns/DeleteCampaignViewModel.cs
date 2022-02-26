using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Campaigns
{
    [DataContract]
    public class DeleteCampaignViewModel
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }        
    }
}
