using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class CollectedProspectsRequest : BaseHalRequest
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string CampaignId { get; set; }

        [DataMember]
        public string PrimaryProspectListId { get; set; }

        [DataMember]
        public string CampaignProspectListId { get; set; }

        [DataMember]
        public IList<PrimaryProspectRequest> Prospects { get; set; }

    }
}
