using System.Runtime.Serialization;

namespace Leadsly.Domain.MQ.Messages
{
    [DataContract]
    public class DeprovisionResourcesBody
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string HalId { get; set; }
    }
}
