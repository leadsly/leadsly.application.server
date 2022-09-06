using Leadsly.Domain.Models.ProspectList;
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
        public string PrimaryProspectListId { get; set; }
        [DataMember]
        public string SocialAccountId { get; set; }
        [DataMember]
        public string CampaignId { get; set; }
        [DataMember]
        public IList<PersistPrimaryProspectModel> Items { get; set; }
    }
}
