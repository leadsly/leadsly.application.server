using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class FollowUpMessageSentRequest : BaseHalRequest
    {
        [DataMember]
        public string ProspectName { get; set; }
        [DataMember]
        public int MessageOrderNum { get; set; }
        [DataMember]
        public long MessageSentTimestamp { get; set; }

    }
}
