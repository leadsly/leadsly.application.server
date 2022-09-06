using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.FollowUpMessage
{
    [DataContract]
    public class SentFollowUpMessageModel
    {
        [DataMember]
        public int MessageOrderNum { get; set; }
        [DataMember]
        public long ActualDeliveryDateTimeStamp { get; set; }
    }
}
