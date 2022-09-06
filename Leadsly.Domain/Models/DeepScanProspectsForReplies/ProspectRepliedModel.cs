using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.DeepScanProspectsForReplies
{
    [DataContract]
    public class ProspectRepliedModel
    {
        [DataMember]
        public string CampaignProspectId { get; set; }
        [DataMember]
        public string ResponseMessage { get; set; }
        [DataMember]
        public long ResponseMessageTimestamp { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
