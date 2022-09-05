using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class SentFollowUpMessageRequest
    {
        [DataMember]
        public int MessageOrderNum { get; set; }
        [DataMember]
        public long ActualDeliveryDateTimeStamp { get; set; }
    }
}
