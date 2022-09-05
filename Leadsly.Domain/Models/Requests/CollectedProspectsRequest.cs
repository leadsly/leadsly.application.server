using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class CollectedProspectsRequest
    {
        [DataMember]
        public string CampaignProspectListId { get; set; }
        [DataMember]
        public IList<PersistPrimaryProspect> Items { get; set; }
    }
}
