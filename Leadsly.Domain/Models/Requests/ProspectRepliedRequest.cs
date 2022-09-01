using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class ProspectRepliedRequest
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
